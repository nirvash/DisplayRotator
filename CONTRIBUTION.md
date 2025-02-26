# Contributing to DisplayRotator

## リリースプロセス

### 1. バージョン更新

1. プロジェクトファイル（.csproj）の `<Version>` タグを更新
   ```xml
   <Version>1.0.2</Version>
   ```

### 2. コミットとタグ

1. 変更をコミット

   ```bash
   git add .
   git commit -m "Bump version to 1.0.2"
   ```

2. タグを作成
   ```bash
   git tag v1.0.2
   ```

### 3. プッシュ

1. master ブランチに変更をプッシュ

   ```bash
   git push origin master
   ```

2. タグをプッシュ
   ```bash
   git push origin v1.0.2
   ```

#### タグのバージョニングガイドライン

- `major.minor.patch` 形式 (例: v1.0.2)
- メジャー: 大きな変更、互換性のない機能追加
- マイナー: 新機能の追加（下位互換性あり）
- パッチ: バグ修正、小さな調整

### リリース後

- GitHub Actions が自動的にビルドとリリースを実行
- リリースノートが自動生成されます
