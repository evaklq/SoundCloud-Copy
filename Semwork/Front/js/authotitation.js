document.addEventListener("DOMContentLoaded", function() {
    document.querySelector("#submitButton").addEventListener("click", async function (event) {
        event.preventDefault();
        await submit();
    });
});
async function submit() {
    let login = document.getElementsByTagName("input")[0].value;
    let password = document.getElementsByTagName("input")[1].value;
    let errorElement = document.querySelector('.error#authoError');

    // validate user
    if(!validateUserData(login, password)) {
        errorElement.innerText = "Write all fields";
        return
    }
    
    const user = {
        Login: login,
        Password: password
    };
    let url = "http://localhost:2400/autho-user"
    let options = {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(user)
    }
    try {
        let response = await fetch(url, options);
        if (response.ok) {
            const result = await response.text();
            var jsonModel = JSON.parse(result);
        } else {
            alert("Ошибка HTTP: " + response.status);
        }

        const formGroups = document.querySelectorAll('.formGroup');
        formGroups.forEach(group => {
            const inputElement = group.querySelector('input');
            const spanElement = group.querySelector('.error');
            if (inputElement && spanElement) {
                inputElement.value = "";
                spanElement.innerText = "";
                console.log("значения теперь пусты");
            }
        });
        
        if (jsonModel.length === 0) {
            errorElement.innerText = "You log in";
            window.location.href = `/html/profile.html`;
        } else {
            errorElement.innerText = jsonModel[0];
        }
    }
    catch(error){
        alert(error + " фетч")
        console.log("какая-то супер ошибка");
    }
}

function validateUserData(login, password) {
    if(login.length == 0 || password.length == 0) {
        alert("Write all fields")
        return false
    }
    return true
}