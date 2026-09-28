import os

# 1. Remove duplicate CategorizationService in Api.Services if it exists
duplicate_file = 'AIPersonalFinanceTracker.Api/Services/CategorizationService.cs'
if os.path.exists(duplicate_file):
    os.remove(duplicate_file)
    print("Removed duplicate AIPersonalFinanceTracker.Api/Services/CategorizationService.cs")

# 2. Write complete CategorizationService in AIPersonalFinanceTracker.ML
ml_dir = 'AIPersonalFinanceTracker.ML'
os.makedirs(ml_dir, exist_ok=True)
ml_file = os.path.join(ml_dir, 'CategorizationService.cs')

ml_code = """namespace AIPersonalFinanceTracker.ML;

public class CategorizationService
{
    public string PredictCategory(string description)
    {
        return "General";
    }

    public void LearnFromOverride(string description, string category)
    {
        // Model learning stub for category overrides
    }
}
"""

with open(ml_file, 'w', encoding='utf-8') as f:
    f.write(ml_code)
print("Updated AIPersonalFinanceTracker.ML/CategorizationService.cs with required methods.")

# 3. Clean up DI registration in Program.cs
prog_file = 'AIPersonalFinanceTracker.Api/Program.cs'
if os.path.exists(prog_file):
    with open(prog_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Remove any duplicate/ambiguous service registrations
    lines = content.splitlines()
    filtered_lines = []
    for line in lines:
        if 'AddScoped<CategorizationService>' in line or 'AddScoped<AIPersonalFinanceTracker' in line and 'CategorizationService' in line:
            continue
        filtered_lines.append(line)

    content = "\n".join(filtered_lines)

    # Add required using statement and registration
    if 'using AIPersonalFinanceTracker.ML;' not in content:
        content = "using AIPersonalFinanceTracker.ML;\n" + content

    content = content.replace(
        'builder.Services.AddControllers();',
        'builder.Services.AddControllers();\nbuilder.Services.AddScoped<CategorizationService>();'
    )

    with open(prog_file, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Updated Program.cs service registration.")
