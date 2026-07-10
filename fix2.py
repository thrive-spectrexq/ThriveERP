import os
import glob
import re

views_dir = r'C:\Users\frimp\Documents\ThriveERP\src\ThriveERP.Desktop\Views'
for filepath in glob.glob(os.path.join(views_dir, '*.axaml')):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original = content
    
    # We added <Panel> and </Panel> using the previous script but messed up the nesting.
    # The previous script might have left some invalid XML if the </Grid> matches were off.
    # Let's remove the </Panel> and re-insert it properly before the matching </Border>.
    # Instead of regex for the closing tag, let's just do a string replacement.
    
    # We want </Grid> ... </Panel> ... </Border>
    # The previous script did:
    # content = re.sub(r'(</Grid>)\s*(</Border>)\s*(</Grid>|<!-- Tab 3)', r'\1\n                  </Panel>\n                \2\n                \3', content)
    # Wait, the closing tags were:
    #                 </Grid>
    #             </Border>
    
    # I will just write a simple parsing fix or revert and apply manually.
    
    # Let's just strip out all <Panel> and </Panel> we added, and add them back properly.
    if '<Panel>' in content and filepath != r'C:\Users\frimp\Documents\ThriveERP\src\ThriveERP.Desktop\Views\InventoryView.axaml': # InventoryView has a real Panel as root. Wait, better to only replace the ones we added.
        pass

    # Actually, a simpler way is to find the exact spot. 
    # Let's look at the error: The 'Border' start tag on line 54 position 10 does not match the end tag of 'Panel'.
    # That means there is a <Border ...> and then later a </Panel>. The <Panel> was placed AFTER the <Border>.
    # Oh!
    # \1\n <Panel>\n \2 where \1 was <Border ...> and \2 was <TextBlock...
    # So <Border> <Panel> <TextBlock>
    # Then </Grid> </Panel> </Border> ...
    # Wait, in the previous script I did:
    # r'(</Grid>)\s*(</Border>)\s*(</Grid>|<!-- Tab 3)'
    # What if the </Grid> wasn't followed by </Grid> or <!-- Tab 3 ?
    
    # Let's just fix the files directly with Python string replacement for the broken part.
