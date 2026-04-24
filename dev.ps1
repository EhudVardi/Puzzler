# Dev helper for the Puzzler solution.
# Run from the repo root: .\dev.ps1 <command>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Command = "help",
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$Rest
)

$SLN = "Source/Solvers.sln"
$WPF = "Source/Presentation.WPF/Presentation.WPF.csproj"

switch ($Command) {
    "build" {
        dotnet build $SLN @Rest
        exit $LASTEXITCODE
    }
    "run" {
        dotnet run --project $WPF @Rest
        exit $LASTEXITCODE
    }
    "test" {
        dotnet test $SLN --logger "console;verbosity=normal" @Rest
        exit $LASTEXITCODE
    }
    "clean" {
        dotnet clean $SLN
        exit $LASTEXITCODE
    }
    "restore" {
        dotnet restore $SLN
        exit $LASTEXITCODE
    }
    "watch" {
        dotnet watch --project $WPF run
        exit $LASTEXITCODE
    }
    { $_ -in @("help", "--help", "-h", "") } {
        Write-Host @"
Usage: .\dev.ps1 <command> [extra dotnet args...]

Commands:
  build     Build the full solution
  run       Launch the WPF application
  test      Run all xUnit tests
  clean     Remove bin/ and obj/ output
  restore   Restore NuGet packages
  watch     Run WPF app with hot-reload (dotnet watch)
  help      Show this message

Extra args are forwarded to dotnet for build/run/test, e.g.:
  .\dev.ps1 build -c Release
  .\dev.ps1 test --filter Sudoku
"@
    }
    default {
        Write-Error "Unknown command: '$Command'  (run '.\dev.ps1 help' for usage)"
        exit 1
    }
}
