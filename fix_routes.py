import os

index_path = 'AIPersonalFinanceTracker.Client/Pages/Index.razor'

if os.path.exists(index_path):
    with open(index_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Resolve route conflict by pointing Index.razor to /index
    updated = content.replace('@page "/"', '@page "/index"').replace('@page ""', '@page "/index"')
    
    with open(index_path, 'w', encoding='utf-8') as f:
        f.write(updated)
        
    print("Resolved route ambiguity in Index.razor.")
else:
    print("Index.razor not found.")
