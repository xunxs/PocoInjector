#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import json

def test_mapping():
    """测试按钮映射"""
    try:
        with open('button_mapping.json', 'r', encoding='utf-8') as f:
            mapping = json.load(f)
        
        print("按钮映射测试")
        print("=" * 50)
        print(f"已加载 {len(mapping)} 个按钮映射")
        print()
        
        print("按钮列表:")
        for i, (name, info) in enumerate(mapping.items(), 1):
            print(f"{i:2d}. {name}")
            print(f"    原始名称: {info['original_name']}")
            print(f"    位置: ({info['position']['x']:.1f}, {info['position']['y']:.1f})")
            print(f"    索引: {info['index']}")
            print()
        
        # 测试查找功能
        print("测试查找功能:")
        test_names = ["退出按钮", "通知标签1", "清理缓存"]
        
        for name in test_names:
            if name in mapping:
                info = mapping[name]
                print(f"找到 '{name}': 索引 {info['index']}, 位置 ({info['position']['x']:.1f}, {info['position']['y']:.1f})")
            else:
                print(f"未找到 '{name}'")
        
        print("\n映射测试完成！")
        
    except FileNotFoundError:
        print("错误: button_mapping.json 文件不存在")
    except Exception as e:
        print(f"错误: {e}")

if __name__ == "__main__":
    test_mapping()
