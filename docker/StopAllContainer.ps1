"`n""List all containers:"
docker ps -aq

"`n""Stop all running containers:"
docker stop $(docker ps -aq)

"`n""Remove all containers:"
docker rm $(docker ps -aq)

"`n""Remove all images:"
docker rmi $(docker images -q) 