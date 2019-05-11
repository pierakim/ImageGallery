"Run SeqLog through Docker:"
# Run a Seq instance with ephemeral storage and all services on port 5341:
docker run -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

"`n""List all docker images:"
docker images

"`n""List all docker containers:"
docker container ls 