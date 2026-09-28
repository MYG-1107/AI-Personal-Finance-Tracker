import os

# 1. Ensure CategorizationService exists in AIPersonalFinanceTracker.ML
ml_service_dir = 'AIPersonalFinanceTracker.ML'
os.makedirs(ml_service_dir, exist_ok=True)
ml_service_path = os.path.join(ml_service_dir, 'CategorizationService.cs')

ml_code = """namespace AIPersonalFinanceTracker.ML;

public class CategorizationService
{
    public string PredictCategory(string description)
    {
        return "General";
    }
}
"""
with open(ml_service_path, 'w', encoding='utf-8') as f:
    f.write(ml_code)
print("Ensured CategorizationService.cs exists in AIPersonalFinanceTracker.ML.")

# 2. Register AIPersonalFinanceTracker.ML.CategorizationService in Program.cs
program_path = 'AIPersonalFinanceTracker.Api/Program.cs'
if os.path.exists(program_path):
    with open(program_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Ensure ML namespace using directive
    if 'using AIPersonalFinanceTracker.ML;' not in content:
        content = "using AIPersonalFinanceTracker.ML;\n" + content

    # Register CategorizationService in DI container
    if 'CategorizationService' not in content:
        content = content.replace(
            'builder.Services.AddControllers();',
            'builder.Services.AddControllers();\nbuilder.Services.AddScoped<CategorizationService>();'
        )
    elif 'AIPersonalFinanceTracker.ML.CategorizationService' not in content:
        content = content.replace(
            'AddScoped<AIPersonalFinanceTracker.Api.Services.CategorizationService>()',
            'AddScoped<AIPersonalFinanceTracker.ML.CategorizationService>()'
        )

    with open(program_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Registered CategorizationService in Program.cs.")

