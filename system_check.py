#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import socket
import subprocess
import sys

def check_port():
    """检查端口状态"""
    print("检查端口 5001 状态...")
    
    try:
        # 尝试连接
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.settimeout(1)
        result = s.connect_ex(('127.0.0.1', 5001))
        s.close()
        
        if result == 0:
            print("✓ 端口 5001 可以连接")
            return True
        else:
            print("✗ 端口 5001 无法连接")
            return False
    except Exception as e:
        print(f"✗ 检查端口时出错: {e}")
        return False

def check_processes():
    """检查相关进程"""
    print("\n检查相关进程...")
    
    try:
        # 检查 Astral Party 进程
        result = subprocess.run(['tasklist', '/FI', 'IMAGENAME eq Astral Party.exe'], 
                              capture_output=True, text=True)
        
        if 'Astral Party.exe' in result.stdout:
            print("✓ Astral Party 进程正在运行")
        else:
            print("✗ Astral Party 进程未运行")
            
        # 检查端口占用
        result = subprocess.run(['netstat', '-ano'], capture_output=True, text=True)
        lines = result.stdout.split('\n')
        
        port_5001_found = False
        for line in lines:
            if ':5001' in line and 'LISTENING' in line:
                print(f"✓ 端口 5001 被占用: {line.strip()}")
                port_5001_found = True
                break
        
        if not port_5001_found:
            print("✗ 端口 5001 未被占用")
            
    except Exception as e:
        print(f"检查进程时出错: {e}")

def test_plugin_file():
    """检查插件文件"""
    print("\n检查插件文件...")
    
    import os
    plugin_path = r"D:\Program Files (x86)\Steam\steamapps\common\Astral Party\BepInEx\plugins\PocoInjector.dll"
    
    if os.path.exists(plugin_path):
        stat = os.stat(plugin_path)
        size = stat.st_size
        mtime = stat.st_mtime
        
        print(f"✓ 插件文件存在")
        print(f"  路径: {plugin_path}")
        print(f"  大小: {size} 字节")
        print(f"  修改时间: {mtime}")
        
        # 检查文件大小是否合理
        if size < 1000:
            print("⚠ 警告: 插件文件太小，可能有问题")
        elif size > 100000:
            print("⚠ 警告: 插件文件太大，可能有问题")
        else:
            print("✓ 插件文件大小正常")
    else:
        print(f"✗ 插件文件不存在: {plugin_path}")

def main():
    print("PocoInjector 系统检查工具")
    print("=" * 40)
    
    # 检查端口
    port_ok = check_port()
    
    # 检查进程
    check_processes()
    
    # 检查插件文件
    test_plugin_file()
    
    print("\n" + "=" * 40)
    if port_ok:
        print("✓ 端口可以连接，但服务器不响应")
        print("  可能原因:")
        print("  1. 插件未正确加载")
        print("  2. 插件启动时崩溃")
        print("  3. BepInEx 日志中有错误信息")
        print("\n建议:")
        print("  1. 检查 BepInEx 控制台日志")
        print("  2. 查看游戏启动时的错误信息")
        print("  3. 确认插件文件正确部署")
    else:
        print("✗ 端口无法连接")
        print("  可能原因:")
        print("  1. 游戏未运行")
        print("  2. BepInEx 未启动")
        print("  3. 插件未加载")
        print("\n建议:")
        print("  1. 确认游戏正在运行")
        print("  2. 检查 BepInEx 是否正常启动")
        print("  3. 查看 BepInEx 日志")

if __name__ == "__main__":
    main()
