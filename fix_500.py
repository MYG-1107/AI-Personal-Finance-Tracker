import os

api_dir = 'AIPersonalFinanceTracker.Api'
replaced_files = 0

for root, dirs, files in os.walk(api_dir):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            if 'FinanceDbContext' in content:
                content = content.replace('FinanceDbContext', 'AppDbContext')
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(content)
                print(f"Updated: {filepath}")
                replaced_files += 1

print(f"Replacement complete. {replaced_files} file(s) updated.")
