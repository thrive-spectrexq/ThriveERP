import os
import glob
import re

views_dir = r'C:\Users\frimp\Documents\ThriveERP\src\ThriveERP.Desktop\Views'
for filepath in glob.glob(os.path.join(views_dir, '*.axaml')):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original = content
    
    pattern = re.compile(r'(<Border Grid\.Column=\"2\"[^>]*>)\s*(<TextBlock IsVisible=\"\{Binding Selected.*?</Grid>)\s*(</Border>)', re.DOTALL)
    content = pattern.sub(r'\1\n                  <Panel>\n                    \2\n                  </Panel>\n                \3', content)

    if filepath.endswith('SettingsView.axaml'):
        content = re.sub(
            r'<TextBlock Text=\"\{Binding SelectedCategory\}\"[^/]*?BorderThickness=[^/]*?/>',
            r'<StackPanel>\n                            <TextBlock Text="{Binding SelectedCategory}" FontSize="24" FontWeight="Bold" Foreground="#1E293B" Padding="0,0,0,10"/>\n                            <Border Height="2" Background="#3B82F6" Margin="0,0,0,10"/>\n                        </StackPanel>',
            content, flags=re.DOTALL
        )
        
    if original != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Fixed {os.path.basename(filepath)}')
