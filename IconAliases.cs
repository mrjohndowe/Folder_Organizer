using System.Collections.Generic;

namespace FolderOrganizer;

public static class IconAliases
{
    public static readonly Dictionary<string, string> ExtensionToIcon =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Microsoft Access
        ["mdb"] = "access",
        ["accdb"] = "access",

        // Adobe/ActionScript
        ["as"] = "actionscript",
        ["fla"] = "flash",

        // Adobe Illustrator
        ["ai"] = "ai",

        // Affinity Suite
        ["afdesign"] = "affinitydesigner",
        ["afphoto"] = "affinityphoto",
        ["afpub"] = "affinitypublisher",

        // AppleScript
        ["scpt"] = "applescript",
        ["applescript"] = "applescript",

        // Arduino
        ["ino"] = "arduino",

        // ASP.NET
        ["asp"] = "asp",
        ["aspx"] = "aspx",
        ["ashx"] = "aspx",
        ["asmx"] = "aspx",
        ["axd"] = "aspx",

        // Assembly
        ["asm"] = "assembly",
        ["s"] = "assembly",
        ["S"] = "assembly",

        // AutoHotkey
        ["ahk"] = "autohotkey",

        // AutoIt
        ["au3"] = "autoit",

        // Text files
        ["txt"] = "text",
        ["text"] = "text",
        ["log"] = "log",
        ["rst"] = "text",
        ["cfg"] = "config",
        ["conf"] = "config",
        ["ini"] = "ini",
        ["env"] = "dotenv",

        // Images
        ["jpg"] = "image",
        ["jpeg"] = "image",
        ["png"] = "image",
        ["gif"] = "image",
        ["bmp"] = "image",
        ["tiff"] = "image",
        ["tif"] = "image",
        ["webp"] = "image",
        ["ico"] = "image",
        ["svg"] = "svg",
        ["avif"] = "avif",

        // Video
        ["mp4"] = "video",
        ["avi"] = "video",
        ["mov"] = "video",
        ["wmv"] = "video",
        ["flv"] = "video",
        ["webm"] = "video",
        ["mkv"] = "video",
        ["m4v"] = "video",
        ["3gp"] = "video",
        ["mpg"] = "video",
        ["mpeg"] = "video",

        // Audio
        ["mp3"] = "audio",
        ["wav"] = "audio",
        ["flac"] = "audio",
        ["aac"] = "audio",
        ["ogg"] = "audio",
        ["wma"] = "audio",
        ["m4a"] = "audio",
        ["alac"] = "audio",

        // Archives
        ["zip"] = "zip",
        ["rar"] = "zip",
        ["7z"] = "zip",
        ["tar"] = "zip",
        ["gz"] = "zip",
        ["bz2"] = "zip",
        ["xz"] = "zip",
        ["tgz"] = "zip",

        // C/C++
        ["c"] = "c",
        ["h"] = "cheader",
        ["cpp"] = "cpp",
        ["cxx"] = "cpp",
        ["cc"] = "cpp",
        ["hpp"] = "cppheader",
        ["hxx"] = "cppheader",
        ["hh"] = "cppheader",

        // C#
        ["cs"] = "csharp",
        ["csx"] = "csharp",

        // C# Projects
        ["csproj"] = "csproj",
        ["sln"] = "sln",
        ["sln2"] = "sln",

        // Java
        ["java"] = "java",
        ["jar"] = "jar",
        ["war"] = "jar",
        ["ear"] = "jar",

        // JavaScript/TypeScript
        ["js"] = "js",
        ["jsx"] = "reactjs",
        ["ts"] = "typescript",
        ["tsx"] = "reactts",
        ["mjs"] = "js",
        ["cjs"] = "js",
        ["mts"] = "typescript",
        ["cts"] = "typescript",

        // JSON
        ["json"] = "json",
        ["jsonc"] = "json",
        ["json5"] = "json5",
        ["jsonld"] = "jsonld",

        // Python
        ["py"] = "python",
        ["pyw"] = "python",
        ["pyi"] = "python",
        ["pyc"] = "python",
        ["pyd"] = "python",
        ["pyo"] = "python",

        // PHP
        ["php"] = "php",
        ["phtml"] = "php",
        ["php3"] = "php",
        ["php4"] = "php",
        ["php5"] = "php",
        ["php7"] = "php",
        ["phps"] = "php",

        // Ruby
        ["rb"] = "ruby",
        ["rbw"] = "ruby",
        ["rake"] = "rake",
        ["gem"] = "ruby",
        ["gemspec"] = "ruby",

        // Go
        ["go"] = "go",

        // Rust
        ["rs"] = "rust",
        ["toml"] = "toml",

        // Swift
        ["swift"] = "swift",

        // Kotlin
        ["kt"] = "kotlin",
        ["kts"] = "kotlin",

        // Dart
        ["dart"] = "dartlang",

        // HTML/CSS
        ["html"] = "html",
        ["htm"] = "html",
        ["css"] = "css",
        ["scss"] = "scss",
        ["sass"] = "sass",
        ["less"] = "less",

        // Markdown
        ["md"] = "markdown",
        ["markdown"] = "markdown",
        ["mdown"] = "markdown",
        ["mkdn"] = "markdown",
        ["mkd"] = "markdown",
        ["mdwn"] = "markdown",
        ["mdtxt"] = "markdown",
        ["mdtext"] = "markdown",

        // XML
        ["xml"] = "xml",
        ["xaml"] = "xaml",
        ["xsl"] = "xsl",
        ["xslt"] = "xsl",
        ["xsd"] = "xml",
        ["dtd"] = "dtd",
        ["svg"] = "svg",

        // SQL
        ["sql"] = "sql",
        ["sqlite"] = "sqlite",
        ["sqlite3"] = "sqlite",
        ["db"] = "db",
        ["db3"] = "db",

        // Shell/Batch
        ["sh"] = "shell",
        ["bash"] = "shell",
        ["zsh"] = "shell",
        ["fish"] = "shell",
        ["bat"] = "bat",
        ["cmd"] = "bat",
        ["ps1"] = "powershell",
        ["psm1"] = "powershell_psm",
        ["psd1"] = "powershell_psd",

        // PowerShell
        ["ps1"] = "powershell",
        ["ps1xml"] = "powershell",
        ["psm1"] = "powershell_psm",
        ["psd1"] = "powershell_psd",

        // Docker
        ["dockerfile"] = "docker",
        ["dockerignore"] = "docker",

        // Git
        ["gitignore"] = "git",
        ["gitattributes"] = "git",
        ["gitmodules"] = "git",
        ["gitkeep"] = "git",

        // Node.js
        ["node"] = "node",
        ["nvmrc"] = "node",

        // NPM
        ["npmignore"] = "npm",
        ["npmrc"] = "npm",

        // Yarn
        ["yarn"] = "yarn",
        ["yarnrc"] = "yarn",

        // Bower
        ["bowerrc"] = "bower",

        // Webpack
        ["webpack"] = "webpack",
        ["webpack.config.js"] = "webpack",

        // Babel
        ["babelrc"] = "babel",
        ["babel.config.js"] = "babel",

        // ESLint
        ["eslintrc"] = "eslint",
        ["eslintrc.js"] = "eslint",
        ["eslintrc.json"] = "eslint",
        ["eslintrc.yaml"] = "eslint",
        ["eslintrc.yml"] = "eslint",
        ["eslintignore"] = "eslint",

        // Prettier
        ["prettierrc"] = "prettier",
        ["prettier.config.js"] = "prettier",
        ["prettierignore"] = "prettier",

        // TypeScript
        ["tsconfig.json"] = "tsconfig",
        ["tsconfig"] = "tsconfig",

        // Vue
        ["vue"] = "vue",

        // React
        ["jsx"] = "reactjs",
        ["tsx"] = "reactts",

        // Angular
        ["angular-cli.json"] = "angular",
        ["angular.json"] = "angular",

        // Svelte
        ["svelte"] = "svelte",

        // Markdown
        ["md"] = "markdown",

        // PDF
        ["pdf"] = "pdf",

        // Excel
        ["xls"] = "excel",
        ["xlsx"] = "excel",
        ["xlsm"] = "excel",
        ["xlsb"] = "excel",
        ["csv"] = "excel",

        // Word
        ["doc"] = "word",
        ["docx"] = "word",
        ["docm"] = "word",
        ["dot"] = "word",
        ["dotx"] = "word",
        ["dotm"] = "word",

        // PowerPoint
        ["ppt"] = "powerpoint",
        ["pptx"] = "powerpoint",
        ["pptm"] = "powerpoint",
        ["pps"] = "powerpoint",
        ["ppsx"] = "powerpoint",
        ["ppsm"] = "powerpoint",
        ["pot"] = "powerpoint",
        ["potx"] = "powerpoint",
        ["potm"] = "powerpoint",

        // Outlook
        ["msg"] = "outlook",
        ["pst"] = "outlook",
        ["ost"] = "outlook",

        // OneNote
        ["one"] = "onenote",
        ["onetoc2"] = "onenote",

        // Adobe Photoshop
        ["psd"] = "photoshop",
        ["psb"] = "photoshop",

        // Adobe InDesign
        ["indd"] = "adobe",
        ["indt"] = "adobe",

        // Adobe Premiere
        ["prproj"] = "adobe",

        // Adobe After Effects
        ["aep"] = "adobe",

        // Adobe Dreamweaver
        ["dwt"] = "adobe",

        // Adobe Flash
        ["fla"] = "flash",
        ["as"] = "actionscript",
        ["swf"] = "flash",

        // 3D/Graphics
        ["blend"] = "blender",
        ["fbx"] = "fbx",
        ["obj"] = "binary",
        ["stl"] = "binary",
        ["gltf"] = "gltf",
        ["glb"] = "gltf",

        // CAD
        ["dwg"] = "drawio",
        ["dxf"] = "drawio",
        ["skp"] = "sketch",

        // LaTeX
        ["tex"] = "tex",
        ["bib"] = "tex",
        ["sty"] = "tex",
        ["cls"] = "tex",

        // R
        ["r"] = "r",
        ["rmd"] = "rmd",
        ["rproj"] = "rproj",

        // MATLAB
        ["m"] = "matlab",
        ["mat"] = "matlab",
        ["fig"] = "matlab",

        // Julia
        ["jl"] = "julia",

        // Lua
        ["lua"] = "lua",

        // Perl
        ["pl"] = "perl",
        ["pm"] = "perl",
        ["t"] = "perl",
        ["pod"] = "perl",

        // Haskell
        ["hs"] = "haskell",
        ["lhs"] = "haskell",

        // Scala
        ["scala"] = "scala",
        ["sbt"] = "sbt",

        // Clojure
        ["clj"] = "clojure",
        ["cljs"] = "clojurescript",
        ["cljc"] = "clojure",
        ["edn"] = "clojure",

        // Elixir
        ["ex"] = "elixir",
        ["exs"] = "elixir",

        // Elm
        ["elm"] = "elm",

        // Erlang
        ["erl"] = "erlang",
        ["hrl"] = "erlang",

        // F#
        ["fs"] = "fsharp",
        ["fsi"] = "fsharp",
        ["fsx"] = "fsharp",
        ["fsscript"] = "fsharp",

        // Visual Basic
        ["vb"] = "vb",
        ["vba"] = "vba",
        ["vbhtml"] = "vbhtml",
        ["vbproj"] = "vbproj",

        // PowerShell
        ["ps1"] = "powershell",
        ["ps1xml"] = "powershell",
        ["psm1"] = "powershell_psm",
        ["psd1"] = "powershell_psd",

        // Batch
        ["bat"] = "bat",
        ["cmd"] = "bat",

        // Shell
        ["sh"] = "shell",
        ["bash"] = "shell",
        ["zsh"] = "shell",
        ["csh"] = "shell",
        ["tcsh"] = "shell",
        ["ksh"] = "shell",

        // Make
        ["makefile"] = "make",
        ["mk"] = "make",
        ["mak"] = "make",

        // CMake
        ["cmake"] = "cmake",
        ["cmakelists.txt"] = "cmake",
        ["ctest"] = "cmake",

        // Gradle
        ["gradle"] = "gradle",
        ["gradlew"] = "gradle",

        // Maven
        ["pom.xml"] = "maven",
        ["mvn"] = "maven",

        // Ant
        ["build.xml"] = "ant",

        // NuGet
        ["nupkg"] = "nuget",
        ["nuspec"] = "nuget",

        // Composer
        ["composer.json"] = "composer",
        ["composer.lock"] = "composer",

        // Pip
        ["requirements.txt"] = "pip",
        ["pipfile"] = "pip",
        ["pipfile.lock"] = "pip",

        // Poetry
        ["pyproject.toml"] = "poetry",
        ["poetry.lock"] = "poetry",

        // Cargo
        ["cargo.toml"] = "cargo",
        ["cargo.lock"] = "cargo",

        // Go modules
        ["go.mod"] = "go",
        ["go.sum"] = "go",

        // npm
        ["package.json"] = "npm",
        ["package-lock.json"] = "npm",
        ["yarn.lock"] = "yarn",

        // Bower
        ["bower.json"] = "bower",

        // JSP
        ["jsp"] = "jsp",

        // ASP.NET
        ["aspx"] = "aspx",
        ["ashx"] = "aspx",
        ["asmx"] = "aspx",
        ["svc"] = "aspx",
        ["axd"] = "aspx",

        // ColdFusion
        ["cfm"] = "cfm",
        ["cfc"] = "cfc",

        // Terraform
        ["tf"] = "terraform",
        ["tfvars"] = "terraform",
        ["hcl"] = "terraform",

        // Kubernetes
        ["yaml"] = "yaml",
        ["yml"] = "yaml",

        // Ansible
        ["yml"] = "yaml",
        ["yaml"] = "yaml",

        // Vagrant
        ["vagrantfile"] = "vagrant",

        // Docker Compose
        ["docker-compose.yml"] = "docker",
        ["docker-compose.yaml"] = "docker",

        // CI/CD
        ["travis.yml"] = "travis",
        ["gitlab-ci.yml"] = "gitlab",
        ["circleci"] = "circleci",
        ["jenkinsfile"] = "jenkins",
        ["azure-pipelines.yml"] = "azurepipelines",
        ["appveyor.yml"] = "appveyor",

        // GraphQL
        ["gql"] = "graphql",
        ["graphql"] = "graphql",

        // Protocol Buffers
        ["proto"] = "protobuf",

        // Avro
        ["avsc"] = "avro",
        ["avdl"] = "avro",
        ["avpr"] = "avro",

        // Thift
        ["thrift"] = "binary",

        // Swagger/OpenAPI
        ["swagger.json"] = "swagger",
        ["swagger.yaml"] = "swagger",
        ["openapi.json"] = "swagger",
        ["openapi.yaml"] = "swagger",

        // Postman
        ["postman.json"] = "postman",

        // Insomnia
        ["insomnia.json"] = "postman",

        // GraphQL
        ["graphql"] = "graphql",
        ["gql"] = "graphql",

        // Prisma
        ["prisma"] = "prisma",

        // Database
        ["sql"] = "sql",
        ["db"] = "db",
        ["sqlite"] = "sqlite",
        ["sqlite3"] = "sqlite",
        ["mdb"] = "access",
        ["accdb"] = "access",

        // Licenses
        ["license"] = "license",
        ["licence"] = "license",
        ["copying"] = "license",
        ["unlicense"] = "unlicense",

        // Git
        ["gitignore"] = "git",
        ["gitattributes"] = "git",
        ["gitmodules"] = "git",
        ["gitkeep"] = "git",
        ["git-blame-ignore-revs"] = "git",

        // SVN
        ["svn"] = "subversion",

        // Mercurial
        ["hg"] = "mercurial",

        // Bazaar
        ["bzr"] = "bazaar",

        // Fossil
        ["fossil"] = "fossil",

        // Other version control
        ["cvs"] = "cvs",

        // Editor configs
        ["editorconfig"] = "editorconfig",
        ["eslintrc"] = "eslint",
        ["prettierrc"] = "prettier",
        ["babelrc"] = "babel",

        // IDE configs
        ["idea"] = "idea",
        ["vscode"] = "vscode",
        ["vscodeignore"] = "vscode",
        ["code-workspace"] = "vscode",

        // Build tools
        ["makefile"] = "make",
        ["rakefile"] = "rake",
        ["gulpfile"] = "gulp",
        ["gruntfile"] = "grunt",
        ["webpackfile"] = "webpack",
        ["rollup.config.js"] = "rollup",

        // Testing
        ["spec.js"] = "test",
        ["spec.ts"] = "test",
        ["test.js"] = "test",
        ["test.ts"] = "test",
        ["jest.config.js"] = "jest",
        ["jasmine.json"] = "jasmine",
        ["mocha.opts"] = "mocha",

        // Documentation
        ["readme"] = "markdown",
        ["changelog"] = "markdown",
        ["contributing"] = "markdown",
        ["authors"] = "markdown",
        ["todo"] = "todo",

        // Configuration files
        ["config"] = "config",
        ["conf"] = "config",
        ["cfg"] = "config",
        ["ini"] = "ini",
        ["env"] = "dotenv",
        ["properties"] = "config",

        // Lock files
        ["lock"] = "lock",
        ["package-lock.json"] = "npm",
        ["yarn.lock"] = "yarn",
        ["composer.lock"] = "composer",
        ["cargo.lock"] = "cargo",
        ["poetry.lock"] = "poetry",
        ["pipfile.lock"] = "pip",

        // Backup files
        ["bak"] = "bak",
        ["backup"] = "bak",
        ["old"] = "bak",
        ["tmp"] = "temp",
        ["temp"] = "temp",

        // Log files
        ["log"] = "log",

        // Data files
        ["dat"] = "binary",
        ["data"] = "binary",
        ["bin"] = "binary",

        // Font files
        ["ttf"] = "font",
        ["otf"] = "font",
        ["woff"] = "font",
        ["woff2"] = "font",
        ["eot"] = "font",

        // Certificate files
        ["cer"] = "cert",
        ["crt"] = "cert",
        ["pem"] = "cert",
        ["key"] = "key",
        ["p12"] = "cert",
        ["pfx"] = "cert",

        // Key files
        ["key"] = "key",
        ["pub"] = "key",
        ["asc"] = "key",

        // GPG
        ["gpg"] = "gpg",
        ["asc"] = "gpg",

        // SSH
        ["pem"] = "cert",
        ["ppk"] = "key",

        // Database schemas
        ["schema"] = "db",
        ["ddl"] = "sql",

        // Migration files
        ["migration"] = "sql",
        ["migrate"] = "sql",

        // Seed files
        ["seed"] = "sql",
        ["seeds"] = "sql",

        // Fixture files
        ["fixture"] = "test",
        ["fixtures"] = "test",

        // Markdown variants
        ["md"] = "markdown",
        ["markdown"] = "markdown",
        ["mdown"] = "markdown",
        ["mkdn"] = "markdown",
        ["mkd"] = "markdown",
        ["mdwn"] = "markdown",
        ["mdtxt"] = "markdown",
        ["mdtext"] = "markdown",
        ["rst"] = "text",

        // Text variants
        ["txt"] = "text",
        ["text"] = "text",
        ["rtf"] = "text",
        ["nfo"] = "text",

        // CSV
        ["csv"] = "excel",

        // TSV
        ["tsv"] = "excel",

        // Office Open XML
        ["docx"] = "word",
        ["xlsx"] = "excel",
        ["pptx"] = "powerpoint",

        // OpenDocument
        ["odt"] = "libreoffice_writer",
        ["ods"] = "libreoffice_calc",
        ["odp"] = "libreoffice_impress",
        ["odg"] = "libreoffice_draw",
        ["odb"] = "libreoffice_base",

        // Other office formats
        ["pages"] = "publisher",
        ["numbers"] = "excel",
        ["key"] = "powerpoint",

        // eBooks
        ["epub"] = "epub",
        ["mobi"] = "epub",
        ["azw"] = "epub",
        ["azw3"] = "epub",

        // Comics
        ["cbz"] = "epub",
        ["cbr"] = "epub",
        ["cb7"] = "epub",
        ["cbt"] = "epub",

        // 3D models
        ["obj"] = "binary",
        ["mtl"] = "text",
        ["fbx"] = "fbx",
        ["blend"] = "blender",
        ["max"] = "maya",
        ["ma"] = "maya",
        ["mb"] = "maya",

        // Game development
        ["unity"] = "unity",
        ["unity3d"] = "unity",
        ["prefab"] = "unity",
        ["asset"] = "asset",
        ["meta"] = "asset",

        // Godot
        ["tscn"] = "tscn",
        ["tres"] = "tres",
        ["gd"] = "gdscript",

        // Unreal Engine
        ["uasset"] = "binary",
        ["umap"] = "binary",
        ["upk"] = "binary",

        // Source code maps
        ["map"] = "map",
        ["jsmap"] = "jsmap",
        ["cssmap"] = "cssmap",

        // Source files
        ["c"] = "c",
        ["cpp"] = "cpp",
        ["h"] = "cheader",
        ["hpp"] = "cppheader",
        ["cs"] = "csharp",
        ["java"] = "java",
        ["py"] = "python",
        ["js"] = "js",
        ["ts"] = "typescript",
        ["go"] = "go",
        ["rs"] = "rust",
        ["rb"] = "ruby",
        ["php"] = "php",
        ["swift"] = "swift",
        ["kt"] = "kotlin",
        ["scala"] = "scala",
        ["dart"] = "dartlang",
        ["lua"] = "lua",
        ["r"] = "r",
        ["m"] = "matlab",
        ["jl"] = "julia",
        ["hs"] = "haskell",
        ["ml"] = "ocaml",
        ["fs"] = "fsharp",
        ["vb"] = "vb",
        ["pl"] = "perl",
        ["sh"] = "shell",
        ["sql"] = "sql",
        ["xml"] = "xml",
        ["html"] = "html",
        ["css"] = "css",
        ["scss"] = "scss",
        ["sass"] = "sass",
        ["less"] = "less",
        ["json"] = "json",
        ["yaml"] = "yaml",
        ["toml"] = "toml",
        ["ini"] = "ini",
        ["cfg"] = "config",
        ["conf"] = "config",

        // Web technologies
        ["html"] = "html",
        ["htm"] = "html",
        ["xhtml"] = "html",
        ["css"] = "css",
        ["scss"] = "scss",
        ["sass"] = "sass",
        ["less"] = "less",
        ["styl"] = "stylus",
        ["stylus"] = "stylus",
        ["js"] = "js",
        ["jsx"] = "reactjs",
        ["ts"] = "typescript",
        ["tsx"] = "reactts",
        ["vue"] = "vue",
        ["svelte"] = "svelte",
        ["json"] = "json",
        ["xml"] = "xml",
        ["svg"] = "svg",

        // Frameworks
        ["angular"] = "angular",
        ["react"] = "reactjs",
        ["vue"] = "vue",
        ["svelte"] = "svelte",
        ["next"] = "next",
        ["nuxt"] = "nuxt",
        ["gatsby"] = "gatsby",
        ["ember"] = "ember",
        ["aurelia"] = "aurelia",
        ["meteor"] = "meteor",
        ["polymer"] = "polymer",

        // Build tools
        ["webpack"] = "webpack",
        ["rollup"] = "rollup",
        ["parcel"] = "parcel",
        ["vite"] = "vite",
        ["esbuild"] = "esbuild",
        ["babel"] = "babel",
        ["gulp"] = "gulp",
        ["grunt"] = "grunt",
        ["browserify"] = "node",

        // Testing
        ["jest"] = "jest",
        ["mocha"] = "mocha",
        ["jasmine"] = "jasmine",
        ["karma"] = "karma",
        ["cypress"] = "cypress",
        ["playwright"] = "playwright",
        ["selenium"] = "test",

        // Package managers
        ["npm"] = "npm",
        ["yarn"] = "yarn",
        ["pnpm"] = "pnpm",
        ["bower"] = "bower",
        ["pip"] = "pip",
        ["poetry"] = "poetry",
        ["composer"] = "composer",
        ["cargo"] = "cargo",
        ["go"] = "go",
        ["nuget"] = "nuget",
        ["paket"] = "paket",

        // Cloud platforms
        ["aws"] = "aws",
        ["azure"] = "azure",
        ["gcp"] = "gcloud",
        ["google"] = "gcloud",
        ["heroku"] = "hashicorp",
        ["vercel"] = "vercel",
        ["netlify"] = "netlify",
        ["cloudflare"] = "cloudflare",

        // Containerization
        ["docker"] = "docker",
        ["kubernetes"] = "kubernetes",
        ["k8s"] = "kubernetes",
        ["helm"] = "helm",

        // CI/CD
        ["github"] = "github",
        ["gitlab"] = "gitlab",
        ["bitbucket"] = "bitbucketpipeline",
        ["jenkins"] = "jenkins",
        ["travis"] = "travis",
        ["circleci"] = "circleci",
        ["azure"] = "azurepipelines",
        ["appveyor"] = "appveyor",
        ["drone"] = "drone",
        ["teamcity"] = "jetbrains",

        // Monitoring
        ["prometheus"] = "prometheus",
        ["grafana"] = "prometheus",
        ["datadog"] = "datadog",
        ["newrelic"] = "datadog",
        ["sentry"] = "sentry",

        // Databases
        ["mysql"] = "mysql",
        ["postgresql"] = "pgsql",
        ["postgres"] = "pgsql",
        ["mongodb"] = "mongo",
        ["redis"] = "redis",
        ["elasticsearch"] = "elastic",
        ["cassandra"] = "db",
        ["dynamodb"] = "aws",
        ["firebase"] = "firebase",
        ["supabase"] = "supabase",

        // ORMs
        ["prisma"] = "prisma",
        ["sequelize"] = "sequelize",
        ["typeorm"] = "db",
        ["hibernate"] = "db",
        ["entity"] = "db",

        // API
        ["graphql"] = "graphql",
        ["grpc"] = "protobuf",
        ["rest"] = "rest",
        ["soap"] = "xml",
        ["openapi"] = "swagger",
        ["swagger"] = "swagger",

        // Security
        ["security"] = "key",
        ["auth"] = "key",
        ["oauth"] = "key",
        ["jwt"] = "key",
        ["ssl"] = "cert",
        ["tls"] = "cert",

        // Mobile
        ["android"] = "android",
        ["ios"] = "ios",
        ["react-native"] = "reactjs",
        ["flutter"] = "flutter",
        ["ionic"] = "ionic",
        ["capacitor"] = "capacitor",
        ["cordova"] = "ionic",

        // Desktop
        ["electron"] = "electron",
        ["tauri"] = "tauri",
        ["nwjs"] = "node",

        // IoT
        ["arduino"] = "arduino",
        ["raspberry"] = "raspberry",
        ["esp"] = "arduino",
        ["iot"] = "arduino",

        // Data science
        ["jupyter"] = "jupyter",
        ["notebook"] = "jupyter",
        ["pandas"] = "python",
        ["numpy"] = "numpy",
        ["scipy"] = "python",
        ["tensorflow"] = "python",
        ["pytorch"] = "python",
        ["keras"] = "python",

        // Machine learning
        ["ml"] = "python",
        ["ai"] = "python",
        ["model"] = "model",
        ["training"] = "python",

        // Big data
        ["spark"] = "apache",
        ["hadoop"] = "apache",
        ["kafka"] = "apache",
        ["flink"] = "apache",
        ["hdfs"] = "apache",

        // Streaming
        ["kafka"] = "apache",
        ["kinesis"] = "aws",
        ["pubsub"] = "google",

        // Messaging
        ["rabbitmq"] = "rabbitmq",
        ["activemq"] = "apache",
        ["zeromq"] = "binary",

        // Search
        ["elasticsearch"] = "elastic",
        ["solr"] = "apache",
        ["algolia"] = "search_result",

        // Caching
        ["redis"] = "redis",
        ["memcached"] = "memcached",
        ["varnish"] = "cache",

        // CDN
        ["cloudfront"] = "aws",
        ["fastly"] = "fastly",
        ["akamai"] = "cloudflare",

        // DNS
        ["dns"] = "cloudflare",
        ["route53"] = "aws",

        // Email
        ["email"] = "outlook",
        ["mail"] = "outlook",
        ["smtp"] = "outlook",
        ["imap"] = "outlook",

        // Chat
        ["slack"] = "slack",
        ["discord"] = "discord",
        ["telegram"] = "telegram",
        ["whatsapp"] = "whatsapp",
        ["mattermost"] = "slack",

        // Video conferencing
        ["zoom"] = "video",
        ["teams"] = "microsoft",
        ["meet"] = "google",

        // Social media
        ["twitter"] = "twitter",
        ["facebook"] = "facebook",
        ["instagram"] = "instagram",
        ["linkedin"] = "linkedin",
        ["youtube"] = "youtube",

        // Analytics
        ["analytics"] = "google",
        ["ga"] = "google",
        ["gtm"] = "google",
        ["mixpanel"] = "analytics",
        ["amplitude"] = "analytics",
        ["segment"] = "analytics",

        // A/B testing
        ["optimizely"] = "analytics",
        ["vwo"] = "analytics",
        ["ab"] = "analytics",

        // Feature flags
        ["launchdarkly"] = "feature",
        ["flagsmith"] = "feature",
        ["unleash"] = "feature",

        // Error tracking
        ["sentry"] = "sentry",
        ["bugsnag"] = "sentry",
        ["rollbar"] = "sentry",
        ["airbrake"] = "sentry",

        // Performance monitoring
        ["apm"] = "datadog",
        ["newrelic"] = "datadog",
        ["dynatrace"] = "datadog",

        // Uptime monitoring
        ["pingdom"] = "uptime",
        ["uptimerobot"] = "uptime",
        ["statuspage"] = "status",

        // Logging
        ["log"] = "log",
        ["logging"] = "log",
        ["splunk"] = "log",
        ["logstash"] = "elastic",
        ["fluentd"] = "elastic",

        // Metrics
        ["metrics"] = "prometheus",
        ["grafana"] = "prometheus",
        ["influxdb"] = "db",

        // Tracing
        ["jaeger"] = "opentracing",
        ["zipkin"] = "opentracing",
        ["tempo"] = "grafana",

        // Observability
        ["observability"] = "datadog",
        ["monitoring"] = "datadog",

        // Documentation
        ["docs"] = "docs",
        ["swagger"] = "swagger",
        ["redoc"] = "swagger",
        ["stoplight"] = "swagger",

        // API testing
        ["postman"] = "postman",
        ["insomnia"] = "postman",
        ["soapui"] = "test",

        // Load testing
        ["jmeter"] = "test",
        ["locust"] = "python",
        ["k6"] = "test",

        // Security testing
        ["owasp"] = "security",
        ["zap"] = "security",
        ["burp"] = "security",

        // Penetration testing
        ["metasploit"] = "security",
        ["nmap"] = "security",
        ["wireshark"] = "security",

        // Forensics
        ["forensics"] = "security",
        ["autopsy"] = "security",
        ["volatility"] = "security",

        // Incident response
        ["ir"] = "security",
        ["sir"] = "security",
        ["soc"] = "security",

        // Compliance
        ["gdpr"] = "security",
        ["hipaa"] = "security",
        ["pci"] = "security",
        ["soc2"] = "security",
        ["iso27001"] = "security",

        // Governance
        ["governance"] = "security",
        ["policy"] = "security",
        ["audit"] = "security",

        // Risk management
        ["risk"] = "security",
        ["threat"] = "security",
        ["vulnerability"] = "security",

        // DevSecOps
        ["devsecops"] = "security",
        ["sast"] = "security",
        ["dast"] = "security",
        ["sca"] = "security",

        // Container security
        ["container-security"] = "security",
        ["trivy"] = "trivy",
        ["clair"] = "security",

        // Infrastructure as Code security
        ["iac-security"] = "security",
        ["tfsec"] = "security",
        ["checkov"] = "security",
        ["kics"] = "security",

        // Supply chain security
        ["supply-chain"] = "security",
        ["snyk"] = "snyk",
        ["dependabot"] = "dependabot",
        ["renovate"] = "renovate",

        // Secrets management
        ["secrets"] = "key",
        ["vault"] = "hashicorp",
        ["aws-secrets"] = "aws",
        ["azure-keyvault"] = "azure",
        ["gcp-secret-manager"] = "google",

        // Identity and access management
        ["iam"] = "key",
        ["auth0"] = "key",
        ["okta"] = "key",
        ["cognito"] = "aws",
        ["azure-ad"] = "azure",

        // Certificate management
        ["cert-manager"] = "cert",
        ["letsencrypt"] = "cert",
        ["acme"] = "cert",

        // Key management
        ["kms"] = "aws",
        ["azure-kms"] = "azure",
        ["gcp-kms"] = "google",

        // Encryption
        ["encryption"] = "key",
        ["pgp"] = "gpg",
        ["ssh"] = "key",

        // Backup and recovery
        ["backup"] = "backup",
        ["restore"] = "backup",
        ["disaster-recovery"] = "backup",

        // Business continuity
        ["bcp"] = "backup",
        ["drp"] = "backup",

        // High availability
        ["ha"] = "high-availability",
        ["failover"] = "high-availability",
        ["load-balancing"] = "high-availability",

        // Disaster recovery
        ["dr"] = "backup",
        ["rto"] = "backup",
        ["rpo"] = "backup",

        // Capacity planning
        ["capacity"] = "planning",
        ["scaling"] = "planning",

        // Performance optimization
        ["performance"] = "performance",
        ["optimization"] = "performance",
        ["tuning"] = "performance",

        // Cost optimization
        ["cost"] = "cost",
        ["finops"] = "cost",
        ["budget"] = "cost",

        // Resource management
        ["resources"] = "resources",
        ["quota"] = "resources",
        ["limits"] = "resources",

        // Service level agreements
        ["sla"] = "sla",
        ["slo"] = "sla",
        ["sli"] = "sla",

        // Service level objectives
        ["slo"] = "sla",
        ["error-budget"] = "sla",

        // Service level indicators
        ["sli"] = "sla",
        ["metrics"] = "prometheus",

        // Error budget
        ["error-budget"] = "sla",
        ["burn-rate"] = "sla",

        // Incident management
        ["incident"] = "incident",
        ["escalation"] = "incident",
        ["on-call"] = "incident",

        // Post-incident review
        ["postmortem"] = "incident",
        ["retrospective"] = "incident",

        // Change management
        ["change"] = "change",
        ["release"] = "release",
        ["deployment"] = "deployment",

        // Release management
        ["release"] = "release",
        ["version"] = "release",
        ["changelog"] = "markdown",

        // Deployment strategies
        ["blue-green"] = "deployment",
        ["canary"] = "deployment",
        ["rolling"] = "deployment",

        // Feature toggles
        ["feature-flag"] = "feature",
        ["toggle"] = "feature",

        // A/B testing
        ["ab-test"] = "test",
        ["experiment"] = "test",

        // Configuration management
        ["config"] = "config",
        ["settings"] = "config",
        ["preferences"] = "config",

        // Environment management
        ["environment"] = "environment",
        ["dev"] = "environment",
        ["staging"] = "environment",
        ["prod"] = "environment",

        // Infrastructure management
        ["infrastructure"] = "infrastructure",
        ["terraform"] = "terraform",
        ["cloudformation"] = "aws",
        ["arm"] = "azure",

        // Server management
        ["server"] = "server",
        ["instance"] = "server",
        ["vm"] = "server",

        // Container management
        ["container"] = "docker",
        ["pod"] = "kubernetes",
        ["deployment"] = "kubernetes",

        // Orchestration
        ["orchestration"] = "kubernetes",
        ["scheduler"] = "kubernetes",

        // Service mesh
        ["service-mesh"] = "istio",
        ["istio"] = "istio",
        ["linkerd"] = "linkerd",

        // API gateway
        ["api-gateway"] = "api",
        ["gateway"] = "api",

        // Load balancing
        ["load-balancer"] = "load-balancing",
        ["lb"] = "load-balancing",

        // CDN
        ["cdn"] = "cloudflare",
        ["edge"] = "cloudflare",

        // DNS
        ["dns"] = "cloudflare",
        ["domain"] = "cloudflare",

        // SSL/TLS
        ["ssl"] = "cert",
        ["tls"] = "cert",
        ["certificate"] = "cert",

        // WAF
        ["waf"] = "security",
        ["firewall"] = "security",

        // DDoS protection
        ["ddos"] = "security",
        ["protection"] = "security",

        // Bot protection
        ["bot"] = "bot",
        ["crawler"] = "bot",

        // Rate limiting
        ["rate-limit"] = "api",
        ["throttling"] = "api",

        // API management
        ["api-management"] = "api",
        ["rest"] = "rest",
        ["graphql"] = "graphql",

        // Microservices
        ["microservice"] = "microservice",
        ["service"] = "service",

        // Monolith
        ["monolith"] = "monolith",
        ["legacy"] = "legacy",

        // Serverless
        ["serverless"] = "serverless",
        ["lambda"] = "aws",
        ["functions"] = "azure",

        // Edge computing
        ["edge"] = "cloudflare",
        ["edge-computing"] = "cloudflare",

        // Hybrid cloud
        ["hybrid"] = "cloud",
        ["multi-cloud"] = "cloud",

        // Multi-region
        ["multi-region"] = "cloud",
        ["global"] = "cloud",

        // High availability
        ["ha"] = "high-availability",
        ["redundancy"] = "high-availability",

        // Disaster recovery
        ["dr"] = "backup",
        ["backup"] = "backup",

        // Business continuity
        ["bcp"] = "backup",
        ["continuity"] = "backup",

        // Compliance
        ["compliance"] = "security",
        ["audit"] = "security",

        // Governance
        ["governance"] = "security",
        ["policy"] = "security",

        // Risk management
        ["risk"] = "security",
        ["threat"] = "security",

        // Security
        ["security"] = "security",
        ["cybersecurity"] = "security",

        // DevOps
        ["devops"] = "devops",
        ["cicd"] = "cicd",

        // SRE
        ["sre"] = "sre",
        ["reliability"] = "sre",

        // Cloud native
        ["cloud-native"] = "cloud",
        ["kubernetes"] = "kubernetes",

        // Digital transformation
        ["digital"] = "digital",
        ["transformation"] = "digital",

        // Innovation
        ["innovation"] = "innovation",
        ["research"] = "research",

        // Strategy
        ["strategy"] = "strategy",
        ["planning"] = "planning",

        // Leadership
        ["leadership"] = "leadership",
        ["management"] = "management",

        // Culture
        ["culture"] = "culture",
        ["team"] = "team",

        // Learning
        ["learning"] = "learning",
        ["training"] = "training",

        // Development
        ["development"] = "development",
        ["engineering"] = "engineering",

        // Operations
        ["operations"] = "operations",
        ["ops"] = "operations",

        // Support
        ["support"] = "support",
        ["help"] = "help",

        // Sales
        ["sales"] = "sales",
        ["marketing"] = "marketing",

        // Finance
        ["finance"] = "finance",
        ["accounting"] = "finance",

        // HR
        ["hr"] = "hr",
        ["recruiting"] = "hr",

        // Legal
        ["legal"] = "legal",
        ["compliance"] = "legal",

        // Default fallback for common file types
        ["file"] = "file",
        ["document"] = "file",
        ["data"] = "binary",
        ["script"] = "script",
        ["code"] = "code",
        ["source"] = "source",
    };
}