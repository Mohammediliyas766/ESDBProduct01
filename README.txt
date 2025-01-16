###Projection code


fromAll()
    .when({
        $init: function () {
            return {
                count: 0,
                sum: 0,
                min: null,
                max: null,
                avg: 0,
                products: {}
            };
        },
        ProductCreated: function (state, event) {
            var productId = event.data.Id;
            var price = event.data.Price;

            state.products[productId] = price;
            state.count += 1;
            state.sum += price;
            state.min = state.min === null ? price : Math.min(state.min, price);
            state.max = state.max === null ? price : Math.max(state.max, price);
            state.avg = state.sum / state.count;
        },
        ProductUpdated: function (state, event) {
            var productId = event.data.Id;
            var newPrice = event.data.Price;
            var oldPrice = state.products[productId];

            state.products[productId] = newPrice;
            state.sum = state.sum - oldPrice + newPrice;
            state.min = Math.min(...Object.values(state.products));
            state.max = Math.max(...Object.values(state.products));
            state.avg = state.sum / state.count;
        },
        ProductDeleted: function (state, event) {
            var productId = event.data.Id;
            var price = state.products[productId];

            delete state.products[productId];
            state.count -= 1;
            state.sum -= price;
            if (state.count === 0) {
                state.min = null;
                state.max = null;
                state.avg = 0;
            } else {
                state.min = Math.min(...Object.values(state.products));
                state.max = Math.max(...Object.values(state.products));
                state.avg = state.sum / state.count;
            }
        }
    })
    .transformBy(function (state) {
        // Ensure that min and max are not infinity
        state.min = state.min === Infinity ? null : state.min;
        state.max = state.max === Infinity ? null : state.max;
        return state;
    });

