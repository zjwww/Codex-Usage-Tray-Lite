"""Copy public menu labels and the release version into the static demo."""
from pathlib import Path
import json
import xml.etree.ElementTree as ET

repo = Path(__file__).resolve().parents[1]
version = ET.parse(repo / 'src/CodexUsageTrayLite/CodexUsageTrayLite.csproj').findtext('.//Version')
keys = '''RefreshNow OpenLogin CodexCliLoginHelp UsageSourceMenu RefreshInterval IconStyleMenu IconStyleFiveHourOnly IconStyleWeeklyOnly IconStyleBoth IconStyleSideBySide StartAtLogin ProxySettings Theme ThemeFollowWindows ThemeLight ThemeDark LanguageMenu OpenLogs OpenConfig ClearSession ToolsAndHelp Help Update About Exit MinutesSingularFormat MinutesPluralFormat UsageResetsCountFormat LastUpdatedFormat'''.split()
data = {}
for lang, filename in [('en', 'Strings.resx'), ('zh-CN', 'Strings.zh-CN.resx')]:
    values = {n.attrib['name']: n.findtext('value') for n in ET.parse(repo / 'src/CodexUsageTrayLite/Resources' / filename).getroot().findall('data')}
    data[lang] = {k: values[k].replace('&&', '&') for k in keys}
output = '/* Generated from the application resources; run scripts/generate-menu-demo-data.py. */\n'
output += 'window.CUTL_MENU_VERSION = ' + json.dumps(version) + ';\n'
output += 'window.CUTL_MENU_LABELS = ' + json.dumps(data, ensure_ascii=False, indent=2) + ';\n'
(repo / 'docs/site/menu-labels.js').write_text(output, encoding='utf-8')
print('Generated English and Simplified Chinese menu labels for v' + version)
