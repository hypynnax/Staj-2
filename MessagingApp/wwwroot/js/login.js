var phone_input = document.getElementById('phone_number');
phone_input.addEventListener("input", function () {
    let value = phone_input.value.replace(/\D/g, '');

    if (value.length > 11) {
        value = value.slice(0, 11);
    }

    let formattedValue = '';
    for (let i = 0; i < value.length; i++) {
        if ([4, 7, 9].includes(i)) {
            formattedValue += ' ' + value[i];
        } else {
            formattedValue += value[i];
        }
    }
    phone_input.value = formattedValue;
});
