Windows環境で.NETの実行環境を指定するには、環境変数に,
ASPNETCORE_ENVIRONMENTを設定する必要がある。例えば、PowerShellで以下のコマンドを実行すると、システム全体の環境変数としてASPNETCORE_ENVIRONMENTがProductionに設定されます。

```powershell
[System.Environment]::SetEnvironmentVariable(
  "ASPNETCORE_ENVIRONMENT", "Production", "Machine")
```