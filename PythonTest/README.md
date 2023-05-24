### Python脚本测试

``` bash
pip freeze > requirements.txt
```

#### docker

``` bash
docker build -t python-simple-webapi .

docker run --rm -p 5000:5000 python-simple-webapi
docker run -p 5000:5000 -d python-simple-webapi

```

https://blog.logrocket.com/build-deploy-flask-app-using-docker/