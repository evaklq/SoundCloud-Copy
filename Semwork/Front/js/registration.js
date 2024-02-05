document.addEventListener("DOMContentLoaded", function() {
    document.querySelector("#submitButton").addEventListener("click", async function (event) {
        event.preventDefault();
        await submit();
    });
});
async function submit() {
    let name = document.getElementsByTagName("input")[0].value;
    let email = document.getElementsByTagName("input")[1].value;
    let phoneNumber = document.getElementsByTagName("input")[2].value;
    let nick = document.getElementsByTagName("input")[3].value;
    let password = document.getElementsByTagName("input")[4].value;
    let iconInput = document.getElementById("trackPhoto");
    
    // validate user
    if(!validateUserData(name, email, phoneNumber, nick, password)) {
        return
    }
    
    let iconUrl = await readAndSaveIcon(iconInput);
    const user = {
        FullName: name,
        Nick: nick,
        Email: email,
        Number: phoneNumber,
        Password: password,
        IconUrl: iconUrl,
    };
    let url = "http://localhost:2400/reg-user"
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

        if (jsonModel.length === 0) {
            let errorElement = document.querySelector('.error#authoError');
            errorElement.innerText = "You registered successfully";
            window.location.href = `/html/profile.html`;
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

        for(let i = 0; i < jsonModel.length; i++){
            if(jsonModel[i].split(":")[0] == "Password"){
                let errorElement = document.querySelector('.error#passwordError');
                errorElement.innerText += jsonModel[i].split(":")[1] + "\n";
                console.log("ошибка пароля");
            }
            else if(jsonModel[i].split(":")[0] == "FullName"){
                let errorElement = document.querySelector('.error#nameError');
                errorElement.innerText += jsonModel[i].split(":")[1] + "\n";
                console.log("ошибка имени");
            }
            else if(jsonModel[i].split(":")[0] == "Email"){
                let errorElement = document.querySelector('.error#emailError');
                errorElement.innerText += jsonModel[i].split(":")[1] + "\n";
                console.log("ошибка имеил");

            }
            else if(jsonModel[i].split(":")[0] == "Number"){
                let errorElement = document.querySelector('.error#phoneNumberError');
                errorElement.innerText += jsonModel[i].split(":")[1] + "\n";
                console.log("ошибка номера");
            }
            else if(jsonModel[i].split(":")[0] == "Nick"){
                let errorElement = document.querySelector('.error#nickError');
                errorElement.innerText += jsonModel[i].split(":")[1] + "\n";
                console.log("ошибка номера");
            }
            else {
                console.log("else");
            }
        }
    }
    catch(error){
        alert(error + " фетч")
        console.log("какая-то супер ошибка");
    }
}

async function readAndSaveIcon(iconInput) {
    return new Promise((resolve, reject) => {
        if (iconInput.files.length > 0) {
            let selectedFile = iconInput.files[0];
            let mimeType = selectedFile.type;
            let reader = new FileReader();

            reader.onload = async function (e) {
                let base64String = e.target.result;
                base64String = base64String.replace(/^data:image\/(png|jpeg);base64,/, '');

                try {
                    const iconUrl = await saveFileAndGetLink(base64String, mimeType);
                    resolve(iconUrl);
                } catch (error) {
                    resolve(null);
                }
            };

            reader.readAsDataURL(selectedFile);
        } else {
            resolve(null);
        }
    });
}

async function saveFileAndGetLink(file, type) {
    let saveFileUrl = "http://localhost:2400/saveToApi"
    let optionsSave = {
        method: "POST",
        headers: {
            "Content-Type": type
        },
        body: JSON.stringify(file)
    }
    try {
        const responseSave = await fetch(saveFileUrl, optionsSave)

        if (responseSave.ok) {
            return await responseSave.text();
        } else {
            return null;
            alert("ошибка в сохранении картинки" + responseSave.status)
        }
    } catch (error) {
        return null;
        alert("ошибка в ответе от сохранения картинки" + error)
    }
}

function validateUserData(name, email, phoneNumber, nick, password) {
    if(name.length == 0 || email.length == 0 || phoneNumber.length == 0 || nick.length == 0 || password.length == 0) {
        alert("Write all fields")
        return false
    }
    return true
}