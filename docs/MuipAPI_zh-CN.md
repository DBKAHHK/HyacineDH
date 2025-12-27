<div align="center">
<table>
<td valign="center"><a href="MuipAPI.md"><img src="https://github.com/twitter/twemoji/blob/master/assets/72x72/1f1fa-1f1f8.png" width="16"/> English</td>
 
<td valign="center"><img src="https://em-content.zobj.net/thumbs/120/twitter/351/flag-china_1f1e8-1f1f3.png" width="16"/> 简中</td>
 
<td valign="center"><a href="MuipAPI_zh-TW.md"><img src="https://em-content.zobj.net/thumbs/120/twitter/351/flag-china_1f1e8-1f1f3.png" width="16"/> 繁中</td>
 
<td valign="center"><a href="MuipAPI_ja-JP.md"><img src="https://github.com/twitter/twemoji/blob/master/assets/72x72/1f1ef-1f1f5.png" width="16"/> 日本語</td>
</td>
</table>
</div>

## 💡API帮助

- 自2.3版本开始，支持外部API调用接口
- 总接口为Dispatch接口加上入口，比如你的Dispatch为 http://127.0.0.1:8080 ，请求参数和返回都为json格式
- (1)创建会话接口: http://127.0.0.1:8080/muip/create_session (支持POST)
  - -可选参数：key_type (类型，仅支持PEM或默认XML)
  - -返回示例：
  ```json
  {
    "code": 0,
    "message": "Created!",
    "data": {
        "rsaPublicKey": "***",
        "sessionId": "***",
        "expireTimeStamp": ***
    }
  }
  ```
- (2)授权接口: http://127.0.0.1:8080/muip/auth_admin (支持POST)
  - -必传参数1：SessionId (在创建会话接口请求后获得)
  - -必传参数2：admin_key (在config.json的MuipServer.AdminKey配置，并且经过rsaPublicKey[创建会话接口获取]下RSA[pacs#1]加密)
  - -返回示例：
  ```json
  {
    "code": 0,
    "message": "Authorized admin key successfully!",
    "data": {
        "sessionId": "***",
        "expireTimeStamp": ***
    }
  }
  ```
- (3)提交命令接口: http://127.0.0.1:8080/muip/exec_cmd (支持POST/GET)
  - -必传参数1：SessionId (在创建会话接口请求后获得)
  - -必传参数2：Command (需要执行的命令经过rsaPublicKey[创建会话接口获取]下RSA[pacs#1]加密)
  - -必传参数3：TargetUid (执行命令的玩家UID)
  - -返回示例：
    ```json
    {
      "code": 0,
      "message": "Success",
      "data": {
          "sessionId": "***",
          "message": "*** //base64编码后
      }
    }
    ```
- (4)获取服务器状态接口: http://127.0.0.1:8080/muip/server_information (支持POST/GET)
  - -必传参数1：SessionId (在创建会话接口请求后获得)
  - -返回示例：
   ```json
    {
      "code": 0,
      "message": "Success",
      "data": {
          "onlinePlayers": [
              {
                  "uid": 10001,
                  "name": "KEVIN",
                  "headIconId": 208001
              },
              ....
          ],
          "serverTime": 1720626191,
          "maxMemory": 16002.227,
          "usedMemory": 7938.5547,
         "programUsedMemory": 323
      }
    }
    ```
- (5)获取玩家信息接口: http://127.0.0.1:8080/muip/player_information (支持POST/GET)
  - -必传参数1：SessionId (在创建会话接口请求后获得)
  - -必传参数2：Uid (玩家UID)
  - -返回示例：
   ```json
    {
      "code": 0,
      "message": "Success",
      "data": {
          "uid": 10001,
          "name": "KEVIN",
          "signature": "",
          "headIconId": 208001,
          "curPlaneId": 10001,
          "curFloorId": 10001001,
          "playerStatus": "Explore",
          "stamina": 182,
          "recoveryStamina": 4,
          "assistAvatarList": Array[0],
          "displayAvatarList": Array[0],
          "finishedMainMissionIdList": Array[38],
          "finishedSubMissionIdList": Array[273],
          "acceptedMainMissionIdList": Array[67],
          "acceptedSubMissionIdList": Array[169]
      }
  }
  ```
