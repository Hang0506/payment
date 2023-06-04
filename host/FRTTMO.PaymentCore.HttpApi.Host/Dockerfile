FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
ARG env=Development
ARG version=0.0.1
ENV ENV=${env}
ENV ASPNETCORE_ENVIRONMENT=${env}
ENV VERSION=${version}

COPY . .
COPY nuget.config ./
RUN dotnet restore "/host/FRTTMO.PaymentCore.HttpApi.Host/FRTTMO.PaymentCore.HttpApi.Host.csproj" --configfile nuget.config -nowarn:msb3202,nu1503 --force

RUN dotnet publish "/host/FRTTMO.PaymentCore.HttpApi.Host/FRTTMO.PaymentCore.HttpApi.Host.csproj" --no-restore -c Release -o /out
#RUN dotnet build "/host/FRTTMO.PaymentCore.HttpApi.Host/FRTTMO.PaymentCore.HttpApi.Host.csproj" --no-restore -c Release 
#RUN dotnet build "/src/FRTTMO.PaymentCore.Domain.Shared/FRTTMO.PaymentCore.Domain.Shared.csproj" -c Release -p:Version=$VERSION -o /artifact
#RUN dotnet build "/src/FRTTMO.PaymentCore.HttpApi.Client/FRTTMO.PaymentCore.HttpApi.Client.csproj" -c Release -p:Version=$VERSION -o /artifact
#RUN dotnet build "/src/FRTTMO.PaymentCore.Application.Contracts/FRTTMO.PaymentCore.Application.Contracts.csproj" -c Release -p:Version=$VERSION -o /artifact
#

#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
#ARG version=0.0.1
#ENV VERSION=${version}
#WORKDIR /artifact
#COPY --from=build-env /artifact/*.Domain.Shared.${VERSION}.nupkg /artifact
#COPY --from=build-env /artifact/*.HttpApi.Client.${VERSION}.nupkg /artifact
#COPY --from=build-env /artifact/*.Application.Contracts.${VERSION}.nupkg /artifact
#RUN echo $VERSION
#RUN ls -lsa
##RUN dotnet nuget push *.${VERSION}.nupkg --source http://10.96.254.139:5555/v3/index.json --api-key 186ef76e9d6a723ecb570d4d9c287487d001e5d35f7ed4a313350a407950318e
#ENTRYPOINT ["dotnet", "nuget", "push", "/artifact/*.nupkg"]
#CMD ["--source", "http://10.96.254.139:5555/v3/index.json","--api-key","186ef76e9d6a723ecb570d4d9c287487d001e5d35f7ed4a313350a407950318e"]
##
#
FROM mcr.microsoft.com/dotnet/aspnet:6.0
ENV TZ=Asia/Ho_Chi_Minh
WORKDIR /app
COPY --from=build-env /out /app
ENTRYPOINT ["dotnet", "FRTTMO.PaymentCore.HttpApi.Host.dll"]
