Set-Location ..
docker build -f .\build\continuous-delivery\users.dockerfile -t bhdebrito/jpproject-user-management-ui:prd .
docker push bhdebrito/jpproject-user-management-ui:prd

docker build -f .\build\continuous-delivery\users-docker.dockerfile -t bhdebrito/jpproject-user-management-ui:3.2.0 .
docker push bhdebrito/jpproject-user-management-ui:3.2.0

docker build -f sso.dockerfile -t bhdebrito/jpproject-sso -t bhdebrito/jpproject-sso:3.2.0 .
docker push bhdebrito/jpproject-sso

docker build -f api.dockerfile -t bhdebrito/jpproject-api:latest -t bhdebrito/jpproject-api:3.2.0 .
docker push bhdebrito/jpproject-api