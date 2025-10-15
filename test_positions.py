#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import socket
import json
import time
import sys

class PositionTester:
    """位置信息测试工具"""
    
    def __init__(self):
        self.socket = None
        
    def connect(self):
        """连接到 PocoInjector 服务"""
        try:
            self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.socket.settimeout(5)
            self.socket.connect(('127.0.0.1', 5001))
            print("成功连接到 PocoInjector 服务")
            return True
        except Exception as e:
            print(f"连接失败: {e}")
            return False
    
    def disconnect(self):
        """断开连接"""
        if self.socket:
            self.socket.close()
            self.socket = None
    
    def get_buttons_with_positions(self):
        """获取按钮及其位置信息"""
        try:
            request = {
                "jsonrpc": "2.0",
                "method": "dump",
                "id": 1
            }
            
            message = json.dumps(request)
            self.socket.sendall(message.encode('utf-8'))
            
            # 接收响应
            response_data = b""
            while True:
                chunk = self.socket.recv(4096)
                if not chunk:
                    break
                response_data += chunk
                if len(chunk) < 4096:
                    break
            
            response = json.loads(response_data.decode('utf-8'))
            
            if "error" in response:
                print(f"请求错误: {response['error']}")
                return []
            
            # 过滤出可能的按钮
            buttons = []
            for i, obj in enumerate(response.get("result", [])):
                if self._is_likely_button(obj):
                    obj['index'] = i
                    buttons.append(obj)
            
            print(f"找到 {len(buttons)} 个可能的按钮")
            return buttons
            
        except Exception as e:
            print(f"获取按钮失败: {e}")
            return []
    
    def _is_likely_button(self, obj):
        """判断是否可能是按钮"""
        name = obj.get("name", "").lower()
        
        # 检查名称关键词
        button_keywords = [
            "button", "btn", "click", "tap", "press", 
            "confirm", "cancel", "ok", "yes", "no",
            "start", "play", "pause", "stop", "exit",
            "menu", "settings", "options", "back", "next",
            "submit", "send", "save", "load", "delete",
            "home", "page", "banner", "activity"
        ]
        
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
    
    def analyze_positions(self, buttons):
        """分析位置信息"""
        print("\n位置信息分析:")
        print("=" * 80)
        
        for i, button in enumerate(buttons[:10]):  # 只显示前10个
            print(f"\n{i+1}. {button.get('name', 'Unknown')}")
            
            # 世界坐标
            pos = button.get('position', {})
            print(f"   世界坐标: ({pos.get('x', 0):.3f}, {pos.get('y', 0):.3f}, {pos.get('z', 0):.3f})")
            
            # 局部坐标
            local_pos = button.get('localPosition', {})
            print(f"   局部坐标: ({local_pos.get('x', 0):.3f}, {local_pos.get('y', 0):.3f}, {local_pos.get('z', 0):.3f})")
            
            # RectTransform 信息
            if 'rectPosition' in button:
                rect_pos = button['rectPosition']
                print(f"   UI坐标: ({rect_pos.get('x', 0):.3f}, {rect_pos.get('y', 0):.3f})")
                
                if 'rectSize' in button:
                    rect_size = button['rectSize']
                    print(f"   UI尺寸: ({rect_size.get('x', 0):.3f}, {rect_size.get('y', 0):.3f})")
                
                if 'rectAnchors' in button:
                    anchors = button['rectAnchors']
                    min_anchor = anchors.get('min', {})
                    max_anchor = anchors.get('max', {})
                    print(f"   锚点: min({min_anchor.get('x', 0):.3f}, {min_anchor.get('y', 0):.3f}) max({max_anchor.get('x', 0):.3f}, {max_anchor.get('y', 0):.3f})")
            
            # 组件信息
            components = button.get('components', [])
            component_names = [comp.get('name', '') for comp in components if isinstance(comp, dict)]
            print(f"   组件: {', '.join(component_names)}")
    
    def test(self):
        """运行测试"""
        print("位置信息测试工具")
        print("=" * 50)
        
        if not self.connect():
            return
        
        # 获取按钮
        buttons = self.get_buttons_with_positions()
        if not buttons:
            print("没有找到按钮")
            return
        
        # 分析位置
        self.analyze_positions(buttons)
        
        self.disconnect()

if __name__ == "__main__":
    tester = PositionTester()
    tester.test()
