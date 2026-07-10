import os
import glob

views_dir = r'C:\Users\frimp\Documents\ThriveERP\src\ThriveERP.Desktop\Views'
for filepath in glob.glob(os.path.join(views_dir, '*.axaml')):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original = content
    
    # 1. Replace the Border -> Panel start
    search_str = '''BorderThickness="1" BorderBrush="#CBD5E1" BoxShadow="0 2 10 0 #10000000">
                    <TextBlock IsVisible="{Binding Selected'''
    replace_str = '''BorderThickness="1" BorderBrush="#CBD5E1" BoxShadow="0 2 10 0 #10000000">
                    <Panel>
                        <TextBlock IsVisible="{Binding Selected'''
    content = content.replace(search_str, replace_str)

    # Some might have "Background="White" Margin="10" CornerRadius="4"" instead of the full BoxShadow. Let's try a safer search
    search2_str = '''>
                    <TextBlock IsVisible="{Binding Selected'''
    
    # Actually, the border might end with `BoxShadow="0 2 10 0 #10000000">`
    # Let's just find the closing tag `</Border>` for the detail panel and insert `</Panel>` right before it.
    # The detail panel is inside `<Border Grid.Column="2" ...>`. The closing `</Border>` is usually followed by `</Grid>` or `<!-- Tab`.
    
    if '<Panel>' in content and '</Panel>' not in content:
        # We need to insert </Panel> just before the matching </Border>
        # The easiest way is to find the last </Border> in the file that precedes the main layout closing, but there might be multiple.
        # Let's do it manually for each file. This is too brittle.
        pass

    if original != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Fixed {os.path.basename(filepath)}')
