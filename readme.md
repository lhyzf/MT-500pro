# Linux 下监控 Santak MT-500pro 系列UPS信息
- 原文链接：<http://www.csksoft.net/blog/post/santak_monitor.html>
- UPS 通讯协议参考：<https://wenku.baidu.com/view/a24107c969dc5022abea0030.html>

## Usage
1. Compile
    ```
    gcc upsread.c -o upsread
    ```
2. Open or create
    ```
    vi /etc/rc.local
    ```
3. Add command and logics
    ```
    #!/bin/bash
    /root/upsread
    exit 0
    ```
4. Set the file to executable
    ```
    chmod a+x /etc/rc.local
    ```