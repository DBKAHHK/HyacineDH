## Introduction of Player Components Part

### Why Components?
Components are a way to organize code in a modular fashion, allowing for better separation of concerns and easier maintenance.

### Why Not Put Everything in PlayerInstance Class?
Putting everything in the `PlayerInstance` class would lead to a monolithic design, making it harder to manage and extend. Components allow for a more flexible architecture where functionality can be added or modified without affecting the entire player instance.

### How to Create Components?
  Class `BasePlayerComponent`  
  - Description: Base class for all player components.
  - Usage: Inherit from this class to create a new player component.
  - Parameters: PlayerInstance
  - Example: 
	```csharp
	public class CustomComponent(PlayerInstance player) : BasePlayerComponent(player)
	{
	}
	```
  - Note: Components should be registered in the `PlayerInstance` constructor to ensure they are initialized properly.

### How to Use Components?
  - Components are accessed through the `PlayerInstance` class.
  - Example: 
	```csharp
	var component = player.GetComponent<CustomComponent>();
	```