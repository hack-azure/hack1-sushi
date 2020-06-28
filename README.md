# hack1-sushi: Azure Functionsを使った Stripe の API 実装サンプル

Azure Functions の HttpTrigger を使って Stripe の

- 顧客の新規作成
- インボイスの発行

を行う簡易なサンプルです。

## 実行について

事前に Stripe の Dashboard からAPI のシークレットキーを取得してください。

取得したシークレットキーを、環境変数 `Stripe:SecretKey` にセットする必要があります。ローカルデバッグと Azure Functions 実行で、セットする方法が異なります。

### ローカルデバッグの場合

csproj ファイルと同じディレクトリーに **local.settings.json** ファイルを追加し、以下のように `Stripe:SecretKey` に Stripe のシークレットキーをセットしてください。

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "Stripe:SecretKey": "<取得したシークレットキーを入力>"
  }
}
```

### Azure Functions で実行の場合

Azure Functions で環境変数 `Stripe:SecretKey` を設定してください。設定方法は[こちら](https://docs.microsoft.com/ja-jp/azure/azure-functions/functions-how-to-use-azure-function-app-settings)のドキュメントをご確認ください。
