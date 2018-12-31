### **功能介绍**  
1. 此插件是基于UGUI所做的图文混排功能，常用于聊天系统的表情嵌入;
2. 可支持静/动态表情,支持超链接;
3. 实现原理，是基于UGUI的富文本，使用quad标签进行占位;
4. 使用了Asset文件来存储本地的表情信息;
5. Text根据正则表达式，解析文本，读取相应的表情信息，并在相应位置绘制相应的Sprite;
6. 正则表达式为[图集ID#表情标签]，图集ID为-ID时，表示此标签为超链接，如-1,图集ID为0时，可省略不写;
7. 有同学提过想支持移动端系统自带的表情，我这里只提一个简单的实现思路，集成不看自己的实际需求了，自己备好系统表情的图集，再解析一下当前系统输入表情的正则表达式，然后跟插件一样的嵌入到Text中（这算是正常的集成实现思路么？）;
---
### **使用步骤**  
1. 选择一张表情图片，导入在unity里，并设置为Texture Type为Sprite(2D and UI);
2. 右键选择图片，点击Create/Sprite Asset,创建图集资源;
3. 针对Asset文件，可以设置图集的ID、是否为静态表情，和标签等属性，同为一个动态表情的Sprite应该设置为同一个标签;
4. 点击菜单栏GameObject/UI/Textline,即可创建UI;
5. 在InlineText组件中输入[#emoji_0],即可显示表情;  
---  
### **截图展示**  
![ 标签对应表情](https://github.com/coding2233/TextInlineSprite/blob/master/ShotScreens/tw04_01.gif)  
![聊天示例](https://github.com/coding2233/TextInlineSprite/blob/master/ShotScreens/tw04_02.gif)  
![更新后，功能展示](https://github.com/coding2233/TextInlineSprite/blob/master/ShotScreens/text01.gif)  
![更新后，聊天测试](https://github.com/coding2233/TextInlineSprite/blob/master/ShotScreens/text02.jpg)   
---
