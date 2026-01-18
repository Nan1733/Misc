Windows環境で.NETの実行環境を指定するには、環境変数に,
ASPNETCORE_ENVIRONMENTを設定する必要がある。例えば、PowerShellで以下のコマンドを実行すると、システム全体の環境変数としてASPNETCORE_ENVIRONMENTがProductionに設定されます。

```powershell
[System.Environment]::SetEnvironmentVariable(
  "ASPNETCORE_ENVIRONMENT", "Production", "Machine")
```

AppConfigに以下を追記するのでもいいらしい

```xml
<configuration>
  <system.webServer>
    <aspNetCore processPath="dotnet" arguments=".\YourApp.dll" stdoutLogEnabled="false" ...>
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

## IIS App Poolに書き込み権限を追加する
ログ出力など、アプリケーションから書き込みが必要な場合は、IISのアプリケーションプールに対して、該当フォルダへの書き込み権限を付与する必要があります。

1. エクスプローラーで該当フォルダを右クリック → [プロパティ] → [セキュリティ] タブ → [編集] → [追加] をクリック。
1. IIS AppPool\DefaultAppPool と入力して [名前の確認] を押す。
   ※ DefaultAppPool の部分は、実際に使用しているアプリケーションプール名に置き換えてください（例: IIS AppPool\MyApp）。

2. 追加されたユーザーに対し、[書き込み] (または [変更]) の許可にチェックを入れて OK を押す。

