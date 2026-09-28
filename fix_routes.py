import os

transactions_file = 'AIPersonalFinanceTracker.Client/Pages/Transactions.razor'
home_file = 'AIPersonalFinanceTracker.Client/Pages/Home.razor'

# Ensure Transactions.razor uses @page "/transactions"
if os.path.exists(transactions_file):
    with open(transactions_file, 'r') as f:
        content = f.read()
    lines = content.splitlines()
    cleaned = [line for line in lines if not line.strip().startswith('@page ')]
    new_content = '@page "/transactions"\n' + '\n'.join(cleaned)
    with open(transactions_file, 'w') as f:
        f.write(new_content)

# Ensure Home.razor uses @page "/"
if os.path.exists(home_file):
    with open(home_file, 'r') as f:
        content = f.read()
    lines = content.splitlines()
    cleaned = [line for line in lines if not line.strip().startswith('@page ')]
    new_content = '@page "/"\n' + '\n'.join(cleaned)
    with open(home_file, 'w') as f:
        f.write(new_content)

print("Route collision resolved successfully.")
