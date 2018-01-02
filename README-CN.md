[English](https://github.com/KedamaOvO/OsuRTDataProvider-Release/blob/master/README.md)  
# 这是什么
OsuRTDataProvider是一个 [OsuSync](https://github.com/Deliay/osuSync) 插件.  
实验性的支持Osu!Tourney。  
  
OsuRTDataProvider能实时的从[OSU!](https://osu.ppy.sh)中获取以下内容(只支持Std):
* BeatmapID
* Game Status
* Accuracy
* Health Point
* Combo
* Count 300
* Count 100
* Count 50
* Count Miss
* Song Title
* Mods
* Playing Time

OSU!客户端版本要求: **b20171225.2 之后**

# 怎么使用
1. 下载 [OsuSync](https://github.com/Deliay/osuSync)。
2. 下载 [OsuRTDataProvider](https://github.com/KedamaOvO/OsuRTDataProvider-Release/releases)。
3. 复制OsuRTDataProvider 到 {OsuSync Path}/Plugins 目录下。
4. 运行 OsuSync。

# Config.ini
[OsuRTDataProvider.SettingIni]  
ListenInterval=33 #单位毫秒  
EnableDirectoryImprecisionSearch=True #提升Songs文件夹搜索范围  
EnableTourneyMode=False #启用Tourney模式?(实验性)  
TeamSize=1 #Tourney client的队伍大小
