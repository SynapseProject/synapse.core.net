param (
    [Parameter(Mandatory=$true)]
    [string]$path, #= ".\foo.csproj",
    [Parameter(Mandatory=$true)]
    [string]$versionFile #= ".\AssemblyInfo.xml"
)

$now = [DateTime]::Now

[string]$file = [System.IO.File]::ReadAllText( $path )

function SetVersionInfo {
    param (
        [string] $tag,
        [int]$major = 0,
        [int]$minor = 1,
        [int]$build = 2,
        [int]$revision = 3
    )

    $v = '<{0}>{1}.{2}.{3}.{4}</{0}>' -f $tag, $major, $minor, $build, $revision
    $regex = '<{0}>(?<v>.*)</{0}>' -f $tag
    if ( $file -match $regex ) {
        $file = ([regex]::Replace( $file, $regex, $v ))
    }
    else {
        $regex = '</PropertyGroup>'
        if ( $file -match $regex ) {
            $v = '  {0}{1}  </PropertyGroup>' -f $v, "`r`n"
            $file = ([regex]::Replace( $file, $regex, $v ))
        }
    }

    $file
}


[Xml]$vf = Get-Content $versionFile

$file = SetVersionInfo 'AssemblyVersion' $vf.ai.av.Major $vf.ai.av.Minor $vf.ai.av.Build $vf.ai.av.Revision

#adjust AssemblyFileVersion, Version
if ( $vf.ai.afv.Build.ToString() -eq '#' ) {
    $vf.ai.afv.Build = '{0}{1}' -f $now.ToString( 'yy' ), $now.DayOfYear.ToString( 'D3' )
}
$file = SetVersionInfo 'FileVersion' $vf.ai.afv.Major $vf.ai.afv.Minor $vf.ai.afv.Build $vf.ai.afv.Revision
$file = SetVersionInfo 'Version' $vf.ai.afv.Major $vf.ai.afv.Minor $vf.ai.afv.Build $vf.ai.afv.Revision


#Write-Host $file
[System.IO.File]::SetAttributes( $path, [System.IO.FileAttributes]::Normal );
[System.IO.File]::WriteAllText( $path, $file )
