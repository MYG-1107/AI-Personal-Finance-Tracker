with open('AIPersonalFinanceTracker.Client/Pages/Transactions.razor', 'r') as f:
    content = f.read()

# Replace the field declaration and usage to avoid name collision with class 'Transactions'
content = content.replace('private List<TransactionDto> Transactions = new();', 'private List<TransactionDto> TransactionList = new();')
content = content.replace('Transactions = txResult;', 'TransactionList = txResult;')
content = content.replace('Transactions.OrderByDescending', 'TransactionList.OrderByDescending')
content = content.replace('@if (Transactions != null && Transactions.Any())', '@if (TransactionList != null && TransactionList.Any())')

with open('AIPersonalFinanceTracker.Client/Pages/Transactions.razor', 'w') as f:
    f.write(content)

print("Transactions.razor property name collision resolved.")
