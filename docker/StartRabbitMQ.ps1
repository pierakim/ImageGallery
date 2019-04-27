"Run Rabbit MQ through Docker:"
docker run -d --hostname my-rabbit -p 15672:15672 -p 5672:5672 --name some-rabbit rabbitmq:3-management

"`n""List all docker images:"
docker images

"`n""List all docker containers:"
docker container ls 