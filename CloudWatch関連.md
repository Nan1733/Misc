
最小限のログとメトリクスを収集するCloudWatchエージェントの設定例。
```json
{
  "metrics": {
    "metrics_collected": {
      "Network Interface": {
        "measurement": ["Bytes Total/sec"],
        "metrics_collection_interval": 60,
        "resources": ["*"]
      }
    }
  }
}

```

https://docs.aws.amazon.com/ja_jp/AmazonCloudWatch/latest/logs/CloudWatchLogs-Transformer-CreateAccountLevel.html

LogTransformerを使用して、CloudWatch Logsエージェントの設定を一括で管理する方法について説明しています。