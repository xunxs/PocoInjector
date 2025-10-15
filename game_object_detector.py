#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import socket
import json
import time
import sys

class PocoClient:
    """PocoInjector 客户端类"""
    
    def __init__(self, host='127.0.0.1', port=5001):
        self.host = host
        self.port = port
        self.socket = None
        
    def connect(self):
        """连接到 PocoInjector 服务"""
        try:
            self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.socket.settimeout(10)
            self.socket.connect((self.host, self.port))
            print(f"成功连接到 {self.host}:{self.port}")
            return True
        except Exception as e:
            print(f"连接失败: {e}")
            return False
    
    def disconnect(self):
        """断开连接"""
        if self.socket:
            self.socket.close()
            self.socket = None
    
    def send_request(self, method, params=None, request_id=1):
        """发送 JSON-RPC 请求"""
        request = {
            "jsonrpc": "2.0",
            "method": method,
            "id": request_id
        }
        
        if params is not None:
            request["params"] = params
        
        try:
            message = json.dumps(request)
            self.socket.sendall(message.encode('utf-8'))
            
            # 接收响应
            response_data = self.socket.recv(8192)
            response = json.loads(response_data.decode('utf-8'))
            
            if "error" in response:
                print(f"请求错误: {response['error']}")
                return None
            
            return response.get("result")
            
        except Exception as e:
            print(f"请求失败: {e}")
            return None

class ButtonDetector:
    """按钮检测器"""
    
    def __init__(self, poco_client):
        self.poco_client = poco_client
        self.buttons = []
        self.all_objects = []
    
    def get_all_objects(self):
        """获取所有游戏对象"""
        print("正在获取游戏对象...")
        
        result = self.poco_client.send_request("dump", [])
        
        if result:
            if isinstance(result, str):
                try:
                    # 尝试解析 JSON 字符串
                    self.all_objects = json.loads(result)
                except json.JSONDecodeError:
                    print(f"无法解析 JSON: {result[:200]}...")
                    return []
            elif isinstance(result, list):
                self.all_objects = result
            else:
                print(f"未知的结果类型: {type(result)}")
                return []
        
        print(f"获取到 {len(self.all_objects)} 个游戏对象")
        return self.all_objects
    
    def find_buttons(self):
        """查找所有按钮"""
        buttons = []
        
        for obj in self.all_objects:
            if self._is_button(obj):
                button_info = self._extract_button_info(obj)
                if button_info:
                    buttons.append(button_info)
                    print(f"发现按钮: {button_info['name']}")
        
        return buttons
    
    def find_ui_elements(self):
        """查找所有 UI 元素"""
        ui_elements = []
        
        for obj in self.all_objects:
            if self._is_ui_element(obj):
                ui_info = self._extract_ui_info(obj)
                if ui_info:
                    ui_elements.append(ui_info)
        
        return ui_elements
    
    def _is_button(self, obj):
        """判断是否是按钮"""
        if not isinstance(obj, dict):
            return False
        
        # 检查名称
        name = obj.get("name", "").lower()
        button_keywords = ["button", "btn", "click", "tap", "press", "confirm", "cancel", "ok", "yes", "no"]
        
        if any(keyword in name for keyword in button_keywords):
            return True
        
        # 检查组件
        components = obj.get("components", [])
        for component in components:
            if isinstance(component, dict):
                comp_name = component.get("name", "").lower()
                if "button" in comp_name or "click" in comp_name:
                    return True
        
        return False
    
    def _is_ui_element(self, obj):
        """判断是否是 UI 元素"""
        if not isinstance(obj, dict):
            return False
        
        # 检查组件
        components = obj.get("components", [])
        ui_keywords = ["ui", "canvas", "image", "text", "button", "input", "scroll", "panel"]
        
        for component in components:
            if isinstance(component, dict):
                comp_name = component.get("name", "").lower()
                if any(keyword in comp_name for keyword in ui_keywords):
                    return True
        
        return False
    
    def _extract_button_info(self, obj):
        """提取按钮信息"""
        try:
            info = {
                "name": obj.get("name", "Unknown"),
                "active": obj.get("active", True),
                "tag": obj.get("tag", ""),
                "layer": obj.get("layer", 0),
                "components": obj.get("components", [])
            }
            
            # 位置信息
            position = obj.get("position", {})
            if isinstance(position, dict):
                info["x"] = position.get("x", 0)
                info["y"] = position.get("y", 0)
                info["z"] = position.get("z", 0)
            
            # 缩放信息
            scale = obj.get("scale", {})
            if isinstance(scale, dict):
                info["scale_x"] = scale.get("x", 1)
                info["scale_y"] = scale.get("y", 1)
                info["scale_z"] = scale.get("z", 1)
            
            return info
            
        except Exception as e:
            print(f"提取按钮信息失败: {e}")
            return None
    
    def _extract_ui_info(self, obj):
        """提取 UI 元素信息"""
        try:
            info = {
                "name": obj.get("name", "Unknown"),
                "active": obj.get("active", True),
                "tag": obj.get("tag", ""),
                "layer": obj.get("layer", 0),
                "components": obj.get("components", [])
            }
            
            # 位置信息
            position = obj.get("position", {})
            if isinstance(position, dict):
                info["x"] = position.get("x", 0)
                info["y"] = position.get("y", 0)
                info["z"] = position.get("z", 0)
            
            return info
            
        except Exception as e:
            print(f"提取 UI 信息失败: {e}")
            return None
    
    def save_to_file(self, data, filename):
        """保存数据到文件"""
        try:
            with open(filename, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
            print(f"数据已保存到 {filename}")
        except Exception as e:
            print(f"保存文件失败: {e}")
    
    def print_summary(self, buttons, ui_elements):
        """打印摘要信息"""
        print(f"\n=== 检测结果 ===")
        print(f"总游戏对象: {len(self.all_objects)}")
        print(f"按钮数量: {len(buttons)}")
        print(f"UI 元素数量: {len(ui_elements)}")
        
        if buttons:
            print(f"\n按钮列表:")
            for i, button in enumerate(buttons, 1):
                print(f"{i:2d}. {button['name']}")
                print(f"    位置: ({button.get('x', 0):.1f}, {button.get('y', 0):.1f})")
                print(f"    激活: {button['active']}")
                print(f"    标签: {button['tag']}")
                print(f"    组件: {len(button['components'])} 个")
                print()

def main():
    """主函数"""
    print("PocoInjector 游戏对象检测工具")
    print("=" * 50)
    
    client = PocoClient()
    
    if not client.connect():
        print("请确保游戏正在运行且 PocoInjector 插件已加载")
        return
    
    try:
        # 测试连接
        print("测试连接...")
        ping_result = client.ping()
        if ping_result:
            print(f"连接正常: {ping_result}")
        
        # 获取屏幕尺寸
        print("获取屏幕尺寸...")
        screen_size = client.send_request("getScreenSize")
        if screen_size:
            print(f"屏幕尺寸: {screen_size}")
        
        # 创建检测器
        detector = ButtonDetector(client)
        
        # 获取所有游戏对象
        objects = detector.get_all_objects()
        
        if objects:
            # 检测按钮
            print("\n检测按钮...")
            buttons = detector.find_buttons()
            
            # 检测 UI 元素
            print("检测 UI 元素...")
            ui_elements = detector.find_ui_elements()
            
            # 显示结果
            detector.print_summary(buttons, ui_elements)
            
            # 保存数据
            if buttons:
                detector.save_to_file(buttons, "buttons.json")
            
            if ui_elements:
                detector.save_to_file(ui_elements, "ui_elements.json")
            
            # 保存所有对象
            detector.save_to_file(objects, "all_objects.json")
        
        print("\n检测完成！")
        
    except KeyboardInterrupt:
        print("\n检测已取消")
    except Exception as e:
        print(f"检测过程中发生错误: {e}")
    finally:
        client.disconnect()

if __name__ == "__main__":
    main()
