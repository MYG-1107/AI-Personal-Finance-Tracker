import os, re

program_path = 'AIPersonalFinanceTracker.Api/Program.cs'

if os.path.exists(program_path):
    with open(program_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Ensure required namespaces are present
    usings = [
        "using AIPersonalFinanceTracker.Api.Data;",
        "using AIPersonalFinanceTracker.ML;",
        "using AIPersonalFinanceTracker.Api.Services;",
        "using System.Text.Json.Serialization;"
    ]
    for u in usings:
        if u not in content:
            content = u + "\n" + content

    # 2. Fix AddControllers with JSON options to ignore object reference cycles
    if 'AddJsonOptions' not in content:
        content = re.sub(
            r'builder\.Services\.AddControllers\([^)]*\);?',
            'builder.Services.AddControllers().AddJsonOptions(options => {\n    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;\n});',
            content
        )

    # 3. Ensure DI registrations exist
    if 'AddScoped<CategorizationService>' not in content:
        content = content.replace(
            'builder.Services.AddControllers',
            'builder.Services.AddScoped<CategorizationService>();\nbuilder.Services.AddScoped<ForecastService>();\nbuilder.Services.AddControllers'
        )

    with open(program_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Program.cs successfully configured with IgnoreCycles and DI services.")

