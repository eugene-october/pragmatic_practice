"""Shopping cart logic using camelCase naming convention."""


def addProductToCart(shoppingCart, productId, unitPrice, quantity=1):
    shoppingCart.append(
        {"productId": productId, "unitPrice": unitPrice, "quantity": quantity}
    )
    return shoppingCart


def computeCartTotal(shoppingCart, taxRate=0.0):
    subTotal = sum(item["unitPrice"] * item["quantity"] for item in shoppingCart)
    return round(subTotal + subTotal * taxRate, 2)


def removeProductFromCart(shoppingCart, productId):
    return [item for item in shoppingCart if item["productId"] != productId]


if __name__ == "__main__":
    cart = []
    addProductToCart(cart, 101, 9.99, 3)
    addProductToCart(cart, 102, 15.00, 2)
    print(computeCartTotal(cart, taxRate=0.07))
    print(removeProductFromCart(cart, 101))
