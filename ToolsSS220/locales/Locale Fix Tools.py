import os
import re
from pathlib import Path

# --- CONFIGURATION ---
BASE_PATH = Path(__file__).parent.parent.parent
LOCALE_PATH = BASE_PATH / "Resources" / "Locale"
RU_PATH = LOCALE_PATH / "ru-RU"
EN_PATH = LOCALE_PATH / "en-US"
PROTOTYPES_PATH = BASE_PATH / "Resources" / "Prototypes"
CODE_PATHS = [BASE_PATH / "Content.Shared", BASE_PATH / "Content.Server", BASE_PATH / "Content.Client"]

REPORT_FILE = "audit_report.txt"
INPUT_FILE = Path("input.ftl")
MISSING_FILE = Path("missing.ftl")

# --- REGEX PATTERNS ---
FTL_VAR_PATTERN = re.compile(r'\{\s*.*?\$\s*([a-zA-Z0-9_\-]+).*?\}')
LOC_START_PATTERN = re.compile(r'Loc\.GetString\s*\(\s*([$]?"[^"]+")')
ARG_NAME_IN_BLOCK_PATTERN = re.compile(r'"([a-zA-Z0-9_\-]+)"')

PROTO_ID_FIELD = re.compile(r'^\s+id:\s*([a-zA-Z0-9\-_.]+)', re.MULTILINE)
PROTO_ABSTRACT_FIELD = re.compile(r'^\s+abstract:\s*true', re.MULTILINE)
PROTO_HIDE_MENU_FIELD = re.compile(r'(?:categories:\s*\[.*HideSpawnMenu.*\])|(?:-\s*HideSpawnMenu)', re.MULTILINE) # IDK, do we need translate hidespawn entities?
PROTO_VAL_PATTERN = re.compile(r'^\s*(?:name|desc|suffix):\s*(?:!type:LocId\s+)?["\']?([a-z0-9\-_.]+)["\']?$', re.MULTILINE)
FTL_ATTRIBUTE_PATTERN = re.compile(r'^\s+\.([a-zA-Z0-9_\-]+)\s*=\s*(.*?)(?=\n\s+\.[a-zA-Z0-9_\-]+\s*=|\n[a-zA-Z0-9\-_.]+\s*=|\Z)', re.DOTALL | re.MULTILINE)

IGNORED_VARS = {'gender', 'proper', 'subject', 'object', 'the', 'user', 'target'}

# --- CORE FUNCTIONS ---

def get_ftl_data(target_path):
    """Parses FTL files and extracts keys, variables, and selector info"""
    ftl_data = {}
    id_pattern = re.compile(r'^([a-zA-Z0-9\-_.]+)\s*=\s*(.*?)(?=\n[a-zA-Z0-9\-_.]+\s*=|\Z)', re.DOTALL | re.MULTILINE)
    for root, _, files in os.walk(target_path):
        for file in files:
            if file.endswith(".ftl"):
                try:
                    content = Path(os.path.join(root, file)).read_text(encoding="utf-8-sig").replace('\r\n', '\n')
                    for match in id_pattern.finditer(content):
                        parent_id = match.group(1).strip()
                        body = match.group(2)

                        def parse_block(text):
                            # Ignore lines starting with # (comments)
                            clean = "\n".join([l for l in text.splitlines() if not l.strip().startswith("#")])
                            return {
                                "vars": set(FTL_VAR_PATTERN.findall(clean)),
                                "select": "->" in clean
                            }

                        ftl_data[parent_id] = parse_block(body)
                        for attr_match in FTL_ATTRIBUTE_PATTERN.finditer(body):
                            attr_id = f"{parent_id}.{attr_match.group(1)}"
                            ftl_data[attr_id] = parse_block(attr_match.group(2))
                except Exception: continue
    return ftl_data

def get_code_data():
    """Scans C# files for GetString calls and captures passed arguments"""
    code_data = {}
    for path in CODE_PATHS:
        for root, _, files in os.walk(path):
            for file in files:
                if file.endswith(".cs"):
                    try:
                        content = Path(os.path.join(root, file)).read_text(encoding="utf-8")
                        for match in LOC_START_PATTERN.finditer(content):
                            raw_id = match.group(1).strip()
                            post_match_pos = match.end()
                            post_char = content[post_match_pos:post_match_pos+10].strip()
                            # Skip dynamic keys or concatenated strings
                            if raw_id.startswith('$') or '{' in raw_id or post_char.startswith('+'): continue
                            loc_id = raw_id.replace('"', '')
                            bracket_count = 1
                            in_quotes = False
                            start_pos = match.end()
                            end_pos = start_pos
                            # Balancing brackets to capture the whole argument block
                            for i in range(start_pos, len(content)):
                                char = content[i]
                                if char == '"' and (i == 0 or content[i-1] != '\\'): in_quotes = not in_quotes
                                if not in_quotes:
                                    if char == '(': bracket_count += 1
                                    elif char == ')': bracket_count -= 1
                                if bracket_count == 0:
                                    end_pos = i
                                    break
                            block = content[start_pos:end_pos].strip()
                            args = set(ARG_NAME_IN_BLOCK_PATTERN.findall(block)) if block else set()
                            line = content.count('\n', 0, match.start()) + 1
                            code_data.setdefault(loc_id, []).append({'file': os.path.relpath(os.path.join(root, file), BASE_PATH), 'line': line, 'args': args})
                    except Exception: continue
    return code_data

def get_proto_data():
    """Parses YAML prototypes to find entity IDs and LocId references"""
    proto_keys = {}
    for root, _, files in os.walk(PROTOTYPES_PATH):
        for file in files:
            if file.endswith(".yml"):
                try:
                    rel_path = os.path.relpath(os.path.join(root, file), BASE_PATH)
                    if "GameRules" in rel_path: continue
                    content = Path(os.path.join(root, file)).read_text(encoding="utf-8").replace('\r\n', '\n')
                    blocks = re.split(r'^- type:\s*', content, flags=re.MULTILINE)
                    for block in blocks:
                        lines = block.splitlines()
                        # Only process entity prototypes
                        if not lines or lines[0].strip() != "entity": continue
                        # Skip abstract or hidden entities
                        if PROTO_ABSTRACT_FIELD.search(block) or PROTO_HIDE_MENU_FIELD.search(block): continue
                        id_match = PROTO_ID_FIELD.search(block)
                        if id_match:
                            e_id = id_match.group(1)
                            proto_keys[f"ent-{e_id}"] = rel_path
                            proto_keys[f"ent-{e_id}.desc"] = rel_path
                        # Find other LocId fields (name, desc, suffix)
                        for m in PROTO_VAL_PATTERN.findall(block):
                            if "-" in m or "." in m: proto_keys[m] = rel_path
                except Exception: continue
    return proto_keys

def get_sync_data(path):
    """Utility to collect data for sync operations"""
    data, order, headers = {}, [], {}
    id_pattern = re.compile(r'^([a-zA-Z0-9\-_.]+)\s*=\s*(.*?)(?=\n[a-zA-Z0-9\-_.]+\s*=|\Z)', re.DOTALL | re.MULTILINE)
    for root, _, files in os.walk(path):
        for file in sorted(files):
            if file.endswith(".ftl"):
                p = Path(os.path.join(root, file))
                content = p.read_text(encoding="utf-8-sig").replace('\r\n', '\n')
                file_keys = []
                for m in id_pattern.finditer(content):
                    key, val = m.group(1).strip(), m.group(2).strip()
                    data[key] = val
                    file_keys.append(key)
                    order.append(key)
                if file_keys:
                    headers[file_keys[0]] = f"## --- FILE: {p.relative_to(path)} ---"
    return data, order, headers

def get_map(path):
    """Creates a map: key -> file path"""
    m = {}
    kp = re.compile(r'^([a-zA-Z0-9\-_.]+)\s*=', re.MULTILINE)
    for r, _, fs in os.walk(path):
        for f in fs:
            if f.endswith(".ftl"):
                p = Path(os.path.join(r, f))
                try:
                    content = p.read_text(encoding="utf-8-sig")
                    for k in kp.findall(content):
                        m[k.strip()] = p
                except: continue
    return m

def _update_block(path: Path, loc_id: str, block: str):
    """ Updates the key and all attributes in FTL (name, desc, suffix), preserving indentation and order """
    path.parent.mkdir(parents=True, exist_ok=True)

    if path.exists():
        text = path.read_text(encoding="utf-8").replace('\r\n', '\n')
    else:
        text = ""

    # Split block into lines
    lines = block.strip().splitlines()
    main_line = lines[0].rstrip()
    attr_lines = [l.rstrip() for l in lines[1:] if l.strip()]

    # --- Update main key ---
    key_pattern = re.compile(rf'^{re.escape(loc_id)}\s*=.*$', re.MULTILINE)
    if key_pattern.search(text):
        text = key_pattern.sub(main_line, text)
    else:
        if text and not text.endswith("\n"):
            text += "\n"
        text += main_line + "\n"

    # --- Update attributes ---
    for attr_line in attr_lines:
        attr_match = re.match(r'^(\s*\.\w+)\s*=\s*(.*)$', attr_line)
        if not attr_match:
            continue
        attr_name = attr_match.group(1).strip()

        # Check if attribute already exists under the target key
        pattern = re.compile(
            rf'^{re.escape(loc_id)}\s*=.*\n((?:[ \t]*{re.escape(attr_name)}\s*=.*\n?)*)',
            re.MULTILINE
        )

        if pattern.search(text):
            # Replace existing attribute
            def repl(m):
                main = m.group(0).splitlines()[0] + "\n"
                return main + attr_line + "\n"
            text = pattern.sub(repl, text)
        else:
            # Insert attribute immediately after the main key
            key_line_pattern = re.compile(rf'^{re.escape(loc_id)}\s*=.*$', re.MULTILINE)
            match = key_line_pattern.search(text)
            if match:
                insert_pos = match.end()
                text = text[:insert_pos] + "\n" + attr_line + text[insert_pos:]
            else:
                # Key is missing — insert at the end
                if text and not text.endswith("\n"):
                    text += "\n"
                text += main_line + "\n" + attr_line + "\n"

    path.write_text(text, encoding="utf-8")

def run_distribute():
    if not INPUT_FILE.exists() or INPUT_FILE.stat().st_size == 0:
        print("❌ input.ftl is empty.")
        return

    print("📤 Distributing keys...")
    raw_content = INPUT_FILE.read_text(encoding="utf-8").replace('\r\n', '\n')

    # Remove comments
    lines = raw_content.splitlines()
    clean_input = "\n".join([l for l in lines if not l.strip().startswith("#")])

    # Split into blocks: key + attributes
    kv_pattern = re.compile(r'^([a-zA-Z0-9\-_.]+)\s*=.*(?:\n\s+\..*)*', re.MULTILINE)
    items = {m.group(0).split('=')[0].strip(): m.group(0) for m in kv_pattern.finditer(clean_input)}

    if not items:
        print("⚠️ Keys not found.")
        return

    ru_map = get_map(RU_PATH)
    en_map = get_map(EN_PATH)

    for loc_id, block in items.items():
        # RU
        if loc_id in ru_map:
            _update_block(ru_map[loc_id], loc_id, block)
            print(f"✅ RU (Update): {loc_id}")
        elif loc_id in en_map:
            rel_path = en_map[loc_id].relative_to(EN_PATH)
            target = RU_PATH / rel_path
            target.parent.mkdir(parents=True, exist_ok=True)
            _update_block(target, loc_id, block)
            print(f"📂 RU (From EN path): {loc_id}")
        else:
            target = RU_PATH / "ss220" / "unsorted.ftl"
            target.parent.mkdir(parents=True, exist_ok=True)
            _update_block(target, loc_id, block)
            print(f"⚠️ RU (Unsorted): {loc_id}")

        # EN
        if loc_id in en_map:
            _update_block(en_map[loc_id], loc_id, block)
        elif loc_id in ru_map:
            rel_path = ru_map[loc_id].relative_to(RU_PATH)
            target = EN_PATH / rel_path
            target.parent.mkdir(parents=True, exist_ok=True)
            _update_block(target, loc_id, block)
        else:
            target = EN_PATH / "ss220" / "unsorted.ftl"
            target.parent.mkdir(parents=True, exist_ok=True)
            _update_block(target, loc_id, block)

    INPUT_FILE.write_text("", encoding="utf-8")
    print("✨ Finished. Keys updated without duplicating attributes.")

def run_audit():
    """Checks for argument mismatches, missing code keys, and prototype issues"""
    ftl_locs = get_ftl_data(RU_PATH)
    code_locs = get_code_data()
    proto_locs = get_proto_data()

    report = ["=== SS220 LOCALIZATION AUDIT ===", ""]
    mismatches = 0
    proto_errors = 0
    missing_code_count = 0
    missing_from_code = []

    report.append("--- ARGUMENT MISMATCHES ---")
    for loc_id, occurrences in code_locs.items():
        if loc_id not in ftl_locs:
            for occ in occurrences:
                missing_from_code.append(f"📍 {occ['file']}:{occ['line']} | Key: {loc_id}")
                missing_code_count += 1
            continue

        for occ in occurrences:
            loc_info = ftl_locs[loc_id]
            diff = loc_info["vars"] - occ['args'] - IGNORED_VARS

            if diff:
                mismatches += 1
                # Mark potential selector-related false positives
                prefix = "⚠️ [SELECTOR] " if loc_info.get("select") else "⚠️ "

                report.append(f"📍 {occ['file']}:{occ['line']} | Key: {loc_id}")
                report.append(f"    {prefix}Missing args in C#: {diff}")

                if loc_info.get("select"):
                    report.append("    (Note: Some args might be optional for specific selector branches)")

    report.append("\n--- MISSING PROTOTYPE KEYS ---")
    for p_id, p_file in proto_locs.items():
        if p_id not in ftl_locs and ("ss220" in p_id.lower() or p_id.startswith("ent-")):
            proto_errors += 1
            report.append(f"📍 {p_file} | Missing: {p_id}")

    report.append("\n--- CODE KEYS MISSING IN .FTL ---")
    if missing_from_code:
        report.extend(sorted(missing_from_code))
    else:
        report.append("No missing keys found.")

    report.append("\n" + "="*30)
    report.append(f"TOTAL ERRORS: {mismatches + proto_errors + missing_code_count}")
    report.append(f"- Argument mismatches: {mismatches}")
    report.append(f"- Missing prototype keys: {proto_errors}")
    report.append(f"- Keys missing in FTL: {missing_code_count}")

    Path(REPORT_FILE).write_text("\n".join(report), encoding="utf-8")
    print(f"✅ Audit done. Report saved to {REPORT_FILE}")

def run_sync():
    """Identifies keys present in one locale but missing in the other"""
    ru_data, ru_order, ru_headers = get_sync_data(RU_PATH)
    en_data, en_order, en_headers = get_sync_data(EN_PATH)
    def is_valid(k): return not k.startswith("accent-") and "dataset" not in k.lower()
    content = []

    # Check what is in EN but missing in RU
    missing_ru = [k for k in en_order if k not in ru_data and is_valid(k)]
    if missing_ru:
        content.append("## === MISSING IN RU ===")
        for k in missing_ru:
            if k in en_headers: content.append(f"\n{en_headers[k]}")
            content.append(f"# EN: {en_data[k].replace(chr(10), chr(10)+'# ')}\n{k} =\n")

    # Check what is in RU but missing in EN
    missing_en = [k for k in ru_order if k not in en_data and is_valid(k)]
    if missing_en:
        content.append("\n## === MISSING IN EN ===")
        for k in missing_en:
            if k in ru_headers: content.append(f"\n{ru_headers[k]}")
            content.append(f"# RU: {ru_data[k].replace(chr(10), chr(10)+'# ')}\n{k} =\n")

    if content:
        MISSING_FILE.write_text("\n".join(content).strip(), encoding="utf-8")
        print(f"📝 {MISSING_FILE} updated.")
    elif MISSING_FILE.exists():
        MISSING_FILE.unlink()

def show_help():
    help_text = """
  ИСПОЛЬЗОВАНИЕ:

1. Run Audit:
   - Проверяет, все ли ключи из кода C# и прототипов есть в RU-локали.
   - Сверяет аргументы в коде и в .ftl файлах.
   - Если ключ содержит селектор (->), он помечается тегом [SELECTOR].
   - Результат сохраняется в audit_report.txt.

2. Run Sync:
   - Сравнивает папки en-US и ru-RU.
   - Создает файл missing.ftl со списком ключей, которых нет в одной из локалей.

3. Run Distribute:
   - Берет содержимое из файла input.ftl и кидает его по нужным файлам.
   - Если ключ уже есть в RU - обновляет его, идентично с EN.
   - Если ключа нет в RU, но есть в EN - создает файл по аналогичному пути в RU.
   - Если ключа нет нигде - кидает в ss220/unsorted.ftl в RU и EN.
"""
    print(help_text)
    input()

if __name__ == "__main__":
    if not INPUT_FILE.exists():
        INPUT_FILE.write_text("", encoding="utf-8")

    while True:
        print("\n--- SS220 LOCALIZATION TOOLKIT ---")
        print("1 - Run Audit\n2 - Run Sync\n3 - Run Distribute\n4 - Help\n0 - Exit")
        c = input("\nSelect mode: ").strip()
        if c == '1': run_audit()
        elif c == '2': run_sync()
        elif c == '3': run_distribute()
        elif c == '4': show_help()
        elif c == '0': exit()
