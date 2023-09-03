
# Genearte types
```shell
npx openapi-typescript http://localhost:5193/swagger/v1/swagger.json -o src/schema.ts
```

# Ignore items below
https://github.com/astahmer/typed-openapi
https://www.zodios.org/

https://github.com/astahmer/typed-openapi

pnpx typed-openapi --runtime=zod  --output src/schema.ts http://localhost:5193/swagger/v1/swagger.json

https://openapi-ts.pages.dev/openapi-fetch/
https://orval.dev/guides/svelte-query

cd ui
pnpm build
cd ..
docker rm -f `docker ps -aq` & docker build -f Dockerfile -t gitea.notnulldev.com/notnulldev/dotnet-playground . && docker run -d -p 8080:5000 -p 7777:80 --env ASPNETCORE_URLS=http://+:5000 --env ASPNETCORE_ENVIRONMENT=PRODUCTION --name dotnet-playground gitea.notnulldev.com/notnulldev/dotnet-playground
docker push gitea.notnulldev.com/notnulldev/dotnet-playground:latest

