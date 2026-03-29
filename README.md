# FileEncodingScanner

`FileEncodingScanner` 是一个基于 `WinForms` 的 Windows 桌面工具，用于批量扫描文件编码并执行转码操作，适合整理源码、脚本、配置文件等文本文件的编码格式。

## 功能简介

- 递归扫描指定目录下的文件。
- 使用正则表达式过滤文件名。
- 检测常见文本编码：
  - `ANSI/Unknown`
  - `UTF-8`
  - `UTF-8 BOM`
  - `UTF-16 LE`
  - `UTF-16 BE`
  - `UTF-32 LE`
  - `UTF-32 BE`
- 在列表中展示文件名、完整路径、编码、大小、修改时间。
- 双击结果可直接打开文件。
- 右键菜单支持将选中文件转换为：
  - `ANSI`
  - `UTF-8`
  - `UTF-8 BOM`
- 顶部按钮支持将当前结果列表中的全部文件一键转换为 `UTF-8 BOM`。
- 扫描过程中支持取消。
- 扫描时会跳过重解析点目录，并统计访问失败的文件或目录。

## 运行环境

- Windows
- `.NET 8 SDK`（从源码构建时需要）

## 构建方式

### 方式一：使用仓库自带脚本

在项目根目录执行：

```bat
build.bat
```

如需指定配置：

```bat
build.bat -c Release
```

脚本会执行 `dotnet build`，并将以下产物复制到项目根目录：

- `FileEncodingScanner.exe`
- `FileEncodingScanner.dll`
- `FileEncodingScanner.deps.json`
- `FileEncodingScanner.runtimeconfig.json`
- `FileEncodingScanner.pdb`

### 方式二：使用 dotnet CLI

```powershell
dotnet build .\FileEncodingScanner.csproj
```

## 使用说明

1. 启动 `FileEncodingScanner.exe`。
2. 选择要扫描的根目录。
3. 输入文件名匹配正则表达式。
4. 点击“扫描”。
5. 在结果列表中查看编码分布。
6. 按需执行以下操作：
   - 双击文件：打开文件。
   - 右键选中文件：转换选中项编码。
   - 点击“全部转为 UTF-8 BOM”：批量转换当前列表中的全部文件。

## 正则示例

- 仅扫描 Pascal 文件：`(?i).*\.pas$`
- 扫描常见文本文件：`(?i).*\.(txt|ini|cfg|json|xml|cs)$`
- 扫描所有文件：`.*`

## 编码识别说明

- 工具优先根据 `BOM` 判断编码。
- 对无 `BOM` 的内容，会根据字节特征推断 `UTF-8`、`UTF-16`、`UTF-32`。
- 如果文件内容更像本地 ANSI 文本，或无法可靠判断，会显示为 `ANSI/Unknown`。
- 空文件会被视为 `ANSI/Unknown`。
- 纯 ASCII 内容在没有 `BOM` 时通常也会落入 `ANSI/Unknown`，这是此类启发式检测的正常结果。

## 转码注意事项

- 转码会直接覆盖原文件，不会自动生成备份。
- 建议在批量转换前先提交版本控制或手动备份。
- 当目标编码无法表示源文件中的某些字符时，转换会失败并提示原因。
- 工具读取文件时会尽量容忍其他进程占用，但写入时仍可能因权限或锁定失败。

## 打开文件行为

- 如果系统中安装了 `Notepad++`，工具会优先使用 `Notepad++` 打开文件。
- 如果未检测到 `Notepad++`，则回退为使用系统默认程序打开。

## 项目结构

- [Program.cs](./Program.cs)：程序入口。
- [Form1.cs](./Form1.cs)：主界面与交互逻辑。
- [FileScanner.cs](./FileScanner.cs)：递归扫描与结果汇总。
- [EncodingDetector.cs](./EncodingDetector.cs)：编码识别逻辑。
- [FileEncodingConverter.cs](./FileEncodingConverter.cs)：文件转码逻辑。
- [FileEncodingInfo.cs](./FileEncodingInfo.cs)：编码定义。

## 适用场景

- 老项目源码目录的编码整理。
- 批量将脚本或配置文件统一为 `UTF-8 BOM`。
- 在迁移仓库前快速定位目录中的混合编码文件。
