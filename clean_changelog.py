initial = r"(.*? )?(\d+\.\d+\.\d+)\nbaltdev avatar\n(.*)\n"
initial_sub = r"<VERSION>\n## \1(\2)\n"

entry = r"(Bugfix|Optimization|Addition|Tweak|Overhaul|Refactor|Improvement|Removal|Adjustment)\n(.*)\n"
entry_sub = r"- **\1**: \2\n"

files = r"Files\n\n.*\n\n"
files_sub = r""

import re

with open("raw_changelog.txt", "r") as f:
    raw = f.read()

raw = re.sub(initial, initial_sub, raw)
raw = re.sub(entry, entry_sub, raw)
raw = re.sub(files, files_sub, raw)

entries = [entry.strip() for entry in raw.split("<VERSION>")]

with open("CHANGELOG.md", "w") as f:
    f.write("\n\n".join(entries))
