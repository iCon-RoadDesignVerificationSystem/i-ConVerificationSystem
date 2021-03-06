# i-ConVerificationSystem

# 本照査システムの概要
道路設計照査システム（以下、本システム）は、「LandXML1.2に準じた3次元設計データ交換標準（案）Ver.1.3 –略称：J-LandXML- 平成31年3月 国土交通省大臣官房技術調査課」および「LandXML1.2に準じた3次元設計データ交換標準の運用ガイドライン（案）Ver.1.3 平成31年3月 国土交通省大臣官房技術調査課」に準じたLandXMLを使用し、道路設計における幾何構造を照査するものです。

本照査システムの照査項目は，「国土交通省大臣官房技術調査課監修，道路詳細設計照査要領（平成29年3月改訂版）」に記載の照査項目のうち，幾何構造に関するものの一部です。  
システムに同梱されている基準値は，以下の技術基準に記載の値です。  
1. 道路構造令の解説と運用，平成27年6月，公益社団法人日本道路協会  
1. 道路構造令，平成31年4月25日施行  
1. 設計要領 第四集 幾何構造編，平成28年8月，株式会社高速道路総合技術研究所  
**本システムは単断面の本線を対象とした照査システムのため、分離断面およびランプの照査には対応していません。**  
  
## 本照査システムによる照査の内容  
### 【幅員構成要素の照査】  
1. 中央帯および中央帯側帯の設置判定  
1. 車線数の適正判定  
1. 路肩の設置判定  
1. 路肩側帯の設置判定  
1. 植樹帯の設置判定  
1. 停車帯の設置判定  
1. 歩行者・自転車通行空間の整備形態判定  
  
### 【幅員の照査】  
1. 中央帯および中央帯側帯の幅員照査  
1. 車線幅員の照査  
1. 路肩幅員の照査  
1. 路肩側帯幅員の照査  
1. 植樹帯幅員の照査  
1. 停車帯幅員の照査  
1. 歩道、自転車歩行車道・自転車道・自転車通行帯幅員の照査  
  
### 【横断勾配の照査】  
1. 直線部の横断勾配の照査  
1. 車道の片勾配照査  
1. 路肩の横断勾配（路肩折れの有無）照査  
1. 歩道、自転車歩行者道・自転車道の横断勾配照査  
  
### 【片勾配すりつけの照査】  
1. 片勾配すりつけ率の照査  
1. 排水のために必要な最小すりつけの照査  
1. 片勾配すりつけ区間が緩和区間内となっているかの照査  
1. 直線から緩和区間なしに直接、円曲線に接続する場合に一様なすりつけを行う場合に、直線部1/2、円曲線部1/2の割合ですりつけを行っているかの照査  
1. 緩和区間を持つS型曲線の場合に、横断勾配0の点とKAの差がA/10以下となっているかの照査  
1. 緩和区間を持たないS型曲線の場合に、横断勾配0の点とBC点が一致しているかの照査  
1. 複合円の場合に、小円1/2、大円1/2の割合ですりつけているかの照査  
  
### 【緩勾配区間長の照査】  
1. 道路区分に応じた緩勾配区間長が確保されているかの照査  

# 使用方法、動作環境など
[取扱説明書](Documents/道路設計照査システム_取扱説明書.docx)をご参照ください。

# 開発環境
- Visual studio
- [.Net Framework 4.8](https://dotnet.microsoft.com/download)
- Newtonsoft.Json
  - and related NuGets
- ReactiveProperty
  - and related NuGets

# Contribute
本システムは**プロトタイプ**であるため、不便な機能やバグが存在する可能性があります。  
それらを見つけた場合はIssueにてお知らせください。  
また、i-ConVerificationSystemの開発に貢献いただけるのであれば、Forkしプログラムを修正し、pull-requestを送っていただけると幸いです。  
上記については、商用利用を目的としたシステム開発に対し、pull-requestを強要するものではありません。  
This program is **PROTOTYPE** so you are likely to be confronted with features and bugs that are inconvenient for you, if you found that please inform by issue.
If you could to contribute to i-ConVerificationSystem, first of all Fork it, modify the program, and send a pull-request.  
The above does not imply that pull-requests are mandatory for system development for commercial use.

# License
- [MIT Lisence](LICENSE)