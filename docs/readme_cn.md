# api_upload
> 一个上传文件的接口，可以用podman、apifox、runapi等工具进行文件的上传

> 可以用docker或者kubernets去部署，或者使用kestrel进行托管部署

[![ Build and Test .NET ](https://github.com/newreport/api_upload/actions/workflows/dotnet.yml/badge.svg)](https://github.com/newreport/api_upload/actions/workflows/dotnet.yml)   [![Publish image to Dockerhub](https://github.com/newreport/api_upload/actions/workflows/docker-image.yml/badge.svg)](https://github.com/newreport/api_upload/actions/workflows/docker-image.yml)

<p align="center">
<a href="../README.md">English</a>
</p>

# 使用
> 环境IP为: 7.7.7.7:7777

> 默认key是 "default"

## 设置key
> JavaScript
```JavaScript
var form = new FormData();
form.append("old_key", "default");
form.append("new_key", "newkey");

var settings = {
  "url": "http://7.7.7.7:7777/set?old_key=default&new_key=newkey",
  "method": "POST",
  "timeout": 0,
  "processData": false,
  "mimeType": "multipart/form-data",
  "contentType": false,
  "data": form
};

$.ajax(settings).done(function (response) {
  console.log(response);
});
```
> Python
```python
import requests

url =  "http://7.7.7.7:7777/set?old_key=default&new_key=newkey",

payload={'old_key': 'default','new_key': 'newkey'}
files=[
]
headers = {}
response = requests.request("POST", url, headers=headers, data=payload, files=files)

print(response.text)
```

## 上传文件
> JavaScript
```JavaScript
var form = new FormData();
form.append("file", fileInput.files[0], "/C:/Users/Admin/Downloads/ss.png");
form.append("key", "default");

var settings = {
  "url": "http://7.7.7.7:7777/pos",
  "method": "POST",
  "timeout": 0,
  "processData": false,
  "mimeType": "multipart/form-data",
  "contentType": false,
  "data": form
};

$.ajax(settings).done(function (response) {
  console.log(response);
});
```

> Python
```python
import requests

url = "http://7.7.7.7:7777/pos"

payload={'key': 'default'}
files=[
  ('file',('ss.png',open('/C:/Users/Admin/Downloads/ss.png','rb'),'image/png'))
]
headers = {}

response = requests.request("POST", url, headers=headers, data=payload, files=files)

print(response.text)

```

## 查看上传的文件
浏览器打开地址 http://7.7.7.7:7777/ls


# Docker 部署
[DockerHub](https://hub.docker.com/r/newreport/api_upload)
```bash
docker pull newreport/api_upload

docker run -it -p 7777:80 -v /your_folder/data:/data -d --name api_upload api_upload
```
```yml
version: '3.8'
services:
  nginx:
    restart: always
    image: nginx:alpine
    container_name: nginx
    ports:
      - 7777:80
    volumes:
      - /your_folder/data:/data
```

# Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-upload-deployment # deloyment名称
spec:
  selector:
    matchLabels:
      app: api-upload
  replicas: 2
  template:
    metadata:
      labels:
        app: api-upload
    spec:
      affinity:
        podAntiAffinity:
          requiredDuringSchedulingIgnoredDuringExecution: # 反亲和性部署，以hostname进行区分
          - topologyKey: kubernetes.io/hostname
            labelSelector:
              matchExpressions:
              - key: app
                operator: In
                values:
                - api-upload
      containers:
      - name: api-upload
        image: newreport/api_upload # dockerhub镜像名称
        ports:
        - containerPort: 80
          protocol: TCP
        volumeMounts:
        - name: data
          mountPath: /data
      volumes:
      - name: data
        hostPath: /your_folder/data
---
apiVersion: v1
kind: Service
metadata:
  name: api-upload-service # service名称
spec:
  selector:
    app: api-upload
  ports:
  - name: http
    protocol: TCP
    port: 80
    targetPort: 80
    nodePort: 31111 # nodeport端口
  type: NodePort
```