filter @message not like /^#/ 

# =========================================================
# URLの正規化処理
# =========================================================
| eval url1 = replace(cs-uri-stem, /\b[0-9]{6}\b/, "{CityCode}")
| eval url2 = replace(url1, /\b[0-9]{4}\b/, "{ID:4}")
| eval url3 = replace(url2, /\b[0-9]{5}\b/, "{ID:5}")
| eval url4 = replace(url3, /\b[0-9]{7}\b/, "{ID:7}")
| eval url5 = replace(url4, /\b[0-9]{8}\b/, "{ID:8}")
| eval url6 = replace(url5, /\b[0-9]{9,}\b/, "{ID:9+}")
| eval normalized_url = url6

# =========================================================
# 統計処理（ステータスコード集計 ＋ 処理時間統計）
# =========================================================
| stats 
    count(*) as TotalCount,

    # --- ステータスコードの集計 ---
    # 20x系
    sum(sc-status = 200) as st_200,
    sum(sc-status >= 200 and sc-status < 300 and sc-status != 200) as st_20x_other,
    
    # 30x系
    sum(sc-status = 301) as st_301,
    sum(sc-status = 302) as st_302,
    sum(sc-status = 304) as st_304,
    sum(sc-status >= 300 and sc-status < 400 and sc-status != 301 and sc-status != 302 and sc-status != 304) as st_30x_other,
    
    # 40x系
    sum(sc-status = 400) as st_400,
    sum(sc-status = 401) as st_401,
    sum(sc-status = 403) as st_403,
    sum(sc-status = 404) as st_404,
    sum(sc-status >= 400 and sc-status < 500 and sc-status != 400 and sc-status != 401 and sc-status != 403 and sc-status != 404) as st_40x_other,
    
    # 50x系
    sum(sc-status = 500) as st_500,
    sum(sc-status = 502) as st_502,
    sum(sc-status = 503) as st_503,
    sum(sc-status >= 500 and sc-status < 600 and sc-status != 500 and sc-status != 502 and sc-status != 503) as st_50x_other,

    # --- 処理時間の統計 ---
    sum(time_taken >= 3000) as Over3s_Count,
    sum(time_taken >= 5000) as Over5s_Count,
    min(time_taken) as Min_ms,
    pct(time_taken, 1) as p1_ms,
    pct(time_taken, 25) as p25_ms,
    pct(time_taken, 50) as p50_ms,
    round(avg(time_taken), 2) as Avg_ms,
    pct(time_taken, 75) as p75_ms,
    pct(time_taken, 95) as p95_ms,
    pct(time_taken, 99) as p99_ms,
    max(time_taken) as Max_ms
  by normalized_url

# =========================================================
# フィルタと並び替え
# =========================================================
| filter TotalCount > 10 
| sort p99_ms desc
| limit 100