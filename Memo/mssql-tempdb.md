**InstallPersistSqlState.sql と InstallSqlState.sql の主な違い**

この2つのファイルは、ASP.NETセッション状態を管理するための異なるストレージモードを実装しています：

## **1. セッションデータの保存場所**

### **InstallPersistSqlState.sql（永続モード）**
- セッションデータは専用データベース `[ASPState].dbo.ASPStateTempSessions` に保存
- SQL Serverの再起動後もデータが**永続的に保持**される
- 古くなったデータはSqlServer Agentにより1分間隔で削除される。

### **InstallSqlState.sql（一時モード）**
- セッションデータは `[tempdb].dbo.ASPStateTempSessions` に保存
- SQL Serverの再起動でデータが**自動的に削除**される

## **3. すべてのストアドプロシージャの参照先**

すべてのデータアクセスストアドプロシージャ（`TempGetAppID`、`TempGetStateItem` など）が、それぞれ異なるデータベースを参照：

- **Persist版**: `FROM [ASPState].dbo.ASPStateTempApplications`
- **通常版**: `FROM [tempdb].dbo.ASPStateTempApplications`

## **4. 使用シナリオ**

### **InstallPersistSqlState.sql を使うべき場合：**
- セッションデータの永続性が必要
- SQL Server再起動後もセッションを維持したい

### **InstallSqlState.sql を使うべき場合：**
- セッションデータの永続性が不要
- ディスク容量を節約したい（tempdbは再起動時に削除されるので、DBが肥大化しにくい）

## **5. パフォーマンスへの影響**

- **tempdb版**: わずかに高速（tempdbはログ処理が軽量）
- **ASPState版**: より信頼性が高いが、わずかにオーバーヘッドあり

**重要な注意**: どちらのスクリプトも古いもので、現在はMicrosoftが推奨する `aspnet_regsql.exe` ユーティリティを使用すべきです。