import os
import glob
import re

views_dir = r'C:\Users\frimp\Documents\ThriveERP\src\ThriveERP.Desktop\Views'
for filepath in glob.glob(os.path.join(views_dir, '*.axaml')):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original = content
    
    if '<TextBlock IsVisible="{Binding Selected' in content:
        # Wrap children in Panel
        content = re.sub(
            r'(<Border Grid\.Column=\"2\"[^>]*>)\s*(<TextBlock IsVisible=\"{Binding Selected.*?(?:IsNull|IsNotNull).*?>)', 
            r'\1\n                  <Panel>\n                    \2', 
            content, flags=re.DOTALL
        )
        content = re.sub(
            r'(</Grid>)\s*(</Border>)\s*(</Grid>|<!-- Tab 3)',
            r'\1\n                  </Panel>\n                \2\n                \3',
            content
        )

    if filepath.endswith('SettingsView.axaml'):
        content = re.sub(
            r'<TextBlock Text=\"{Binding SelectedCategory}\" FontSize=\"24\" FontWeight=\"Bold\" Foreground=\"#1E293B\" BorderThickness=\"[^\"]*\" BorderBrush=\"[^\"]*\" Padding=\"[^\"]*\"/>',
            r'<StackPanel>\n                            <TextBlock Text=\"{Binding SelectedCategory}\" FontSize=\"24\" FontWeight=\"Bold\" Foreground=\"#1E293B\" Padding=\"0,0,0,10\"/>\n                            <Border Height=\"2\" Background=\"#3B82F6\" Margin=\"0,0,0,10\"/>\n                        </StackPanel>',
            content
        )
        
    if original != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Fixed {os.path.basename(filepath)}')
