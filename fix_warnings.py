import glob

vms = glob.glob('src/ThriveERP.Desktop/ViewModels/*.cs')
for vm in vms:
    with open(vm, 'r', encoding='utf-8') as f:
        content = f.read()
    
    content = content.replace('private readonly IMediator _mediator;', 'private readonly IMediator _mediator = null!;')
    content = content.replace('private ViewModelBase _currentView;', 'private ViewModelBase _currentView = null!;')
    content = content.replace('App.Services.GetRequiredService', 'App.Services!.GetRequiredService')
    
    with open(vm, 'w', encoding='utf-8') as f:
        f.write(content)
        
print("Fixed warnings in ViewModels")
