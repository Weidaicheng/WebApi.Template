$oldName = 'WebApi.Template'
$newName = $args[0]

if ($null -eq $newName) {
    Write-Host -ForegroundColor Red "No project name specified"
    exit
}
if($newName.ToString().StartsWith(".")) {
    Write-Host -ForegroundColor Red "Project name can not start with '.'"
    exit
}
if($newName.ToString().EndsWith(".")) {
    Write-Host -ForegroundColor Red "Project name can not end with '.'"
    exit
}

# find all *.cs files, replace "WebApi.Template" to "<new name>"
$folders = $PSScriptRoot, "$PSScriptRoot/Controllers", "$PSScriptRoot/Extensions", "$PSScriptRoot/Models"
foreach ($folder in $folders) {
    $allItems = Get-ChildItem $folder | Where-Object { $_.Name.EndsWith('cs') } | ForEach-Object { $_.Name }
    foreach ($item in $allItems) {
        $path = "$folder/$item"
        $content = (Get-Content $path).Replace($oldName, $newName)
        $content | Out-File -FilePath $path
    }
}

# find WebApi.Template.csproj file, rename to <new name>.csproj
Rename-Item -Path "$PSScriptRoot/$oldName.csproj" -NewName "$newName.csproj"