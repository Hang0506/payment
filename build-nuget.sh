
#!/bin/bash
#########################################
# Packaging ABP module, and push to NUGET
#########################################
# RUN EXAMPLE: ./script/package.sh Search 0.3.7
ROOT_PATH=..
BUILD_TARGET_PATH=.sln
NUGET=http://10.96.254.139:5555/v3/index.json
NUGET_API_KEY=186ef76e9d6a723ecb570d4d9c287487d001e5d35f7ed4a313350a407950318e
BUILD_CONFIG=Release
MODULE=PaymentCore
ROOT_NAMESPACE=FRTTMO.${MODULE}

read -p "Enter Version ${MODULE}: " P_VERSION
if [[ -z $P_VERSION ]]
then
	VERSION=$(echo '111 222 33' | awk -F'[<>]' '/<Version>/{print $3}' common.props)
else
	VERSION=$P_VERSION
fi

echo "VERSION ${VERSION}"
dotnet pack ./src/${ROOT_NAMESPACE}.Domain.Shared -c:Release -p:TargetFramework=netstandard2.0 -p:PackageVersion=$VERSION
dotnet pack ./src/${ROOT_NAMESPACE}.Application.Contracts -c:Release -p:TargetFramework=netstandard2.0 -p:PackageVersion=$VERSION
# dotnet pack ./src/${ROOT_NAMESPACE}.HttpApi -c:Release -p:TargetFramework=net6.0 -p:PackageVersion=$VERSION
# dotnet pack ./src/${ROOT_NAMESPACE}.Application -c:Release -p:TargetFramework=net6.0 -p:PackageVersion=$VERSION
# dotnet pack ./src/${ROOT_NAMESPACE}.Domain -c:Release -p:TargetFramework=net6.0 -p:PackageVersion=$VERSION
dotnet pack ./src/${ROOT_NAMESPACE}.HttpApi.Client -c:Release -p:TargetFramework=net6.0 -p:PackageVersion=$VERSION
# Change directory to code-base root path
#cd ${ROOT_PATH}
# Set privileges __________________________
# chmod 777
# Push _____________________________________________________
echo -e "> Push to Nuget..."
dotnet nuget push ./src/${ROOT_NAMESPACE}.Domain.Shared/bin/${BUILD_CONFIG}/${ROOT_NAMESPACE}.Domain.Shared.${VERSION}.nupkg --source ${NUGET} --api-key ${NUGET_API_KEY}
dotnet nuget push ./src/${ROOT_NAMESPACE}.Application.Contracts/bin/${BUILD_CONFIG}/${ROOT_NAMESPACE}.Application.Contracts.${VERSION}.nupkg --source ${NUGET} --api-key ${NUGET_API_KEY}
# dotnet nuget push ./src/${ROOT_NAMESPACE}.HttpApi/bin/${BUILD_CONFIG}/${ROOT_NAMESPACE}.HttpApi.${VERSION}.nupkg --source ${NUGET} --api-key ${NUGET_API_KEY}
# dotnet nuget push ./src/${ROOT_NAMESPACE}.Application/bin/${BUILD_CONFIG}/${ROOT_NAMESPACE}.Application.${VERSION}.nupkg --source ${NUGET} --api-key ${NUGET_API_KEY}
# dotnet nuget push ./src/${ROOT_NAMESPACE}.Domain/bin/${BUILD_CONFIG}/${ROOT_NAMESPACE}.Domain.${VERSION}.nupkg --source ${NUGET} --api-key ${NUGET_API_KEY}
dotnet nuget push ./src/${ROOT_NAMESPACE}.HttpApi.Client/bin/${BUILD_CONFIG}/${ROOT_NAMESPACE}.HttpApi.Client.${VERSION}.nupkg --source ${NUGET} --api-key ${NUGET_API_KEY}