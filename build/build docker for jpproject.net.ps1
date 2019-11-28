Set-Location ..
docker build -f .\build\continuous-delivery\users.dockerfile -t bhdebrito/jpproject-user-management-ui:prd .
docker push bhdebrito/jpproject-user-management-ui:prd

docker build -f sso.dockerfile -t bhdebrito/jpproject-sso .
docker push bhdebrito/jpproject-sso

docker build -f api.dockerfile -t bhdebrito/jpproject-api .
docker push bhdebrito/jpproject-api