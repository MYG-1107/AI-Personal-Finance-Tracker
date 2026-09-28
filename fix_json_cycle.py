import os, re

program_path = 'AIPersonalFinanceTracker.Api/Program.cs'

if os.path.exists(program_path):
    with open(program_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Add JSON Serialization namespace
    if 'System.Text.Json.Serialization' not in content:
        content = "using System.Text.Json.Serialization;\n" + content

    # 2. Add IgnoreCycles to AddControllers()
    if 'IgnoreCycles' not in content:
        content = re.sub(
            r'builder\.Services\.AddControllers\(\s*\);',
            'builder.Services.AddControllers().AddJsonOptions(options => {\n    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;\n});',
            content
        )

    with open(program_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Configured ReferenceHandler.IgnoreCycles in Program.cs.")
else:
    print("Program.cs not found.")
