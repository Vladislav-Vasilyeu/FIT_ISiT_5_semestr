<?php defined('_JEXEC') or die; ?>
<div id="contactToggleModule">
    <div id="contactButton" style="background:<?php echo $buttonColor; ?>">
        Написать администрации
    </div>

    <div id="contactForm" style="display:none;">
        <div class="contact-header" style="background:<?php echo $buttonColor; ?>">
            Форма обратной связи
        </div>
        <form id="contactFormReal">
            <input type="text" name="contact_name" placeholder="Ваше имя" required />
            <input type="text" name="contact_subject" placeholder="Тема обращения" required />
            <textarea name="contact_message" placeholder="Текст сообщения" required></textarea>
            
            <div class="captcha">
                <span>2 + 2 = ?</span>
                <input type="text" name="contact_captcha" required style="width:60px;" />
            </div>
            
            <button type="submit">Отправить</button>
            <div id="contactResult"></div>
        </form>
    </div>
</div>

<style>
#contactToggleModule{font-family:Arial,sans-serif;max-width:350px;margin:20px auto;}
#contactButton{background:#ff6b35;color:white;padding:15px 20px;text-align:center;cursor:pointer;border-radius:8px;font-weight:bold;box-shadow:0 4px 10px rgba(0,0,0,0.3);transition:all 0.3s;}
#contactButton:hover{transform:scale(1.05);}
#contactForm{background:white;border:2px solid <?php echo $buttonColor; ?>;border-radius:10px;overflow:hidden;box-shadow:0 10px 30px rgba(0,0,0,0.2);}
.contact-header{padding:15px;color:white;text-align:center;font-weight:bold;}
#contactForm input,#contactForm textarea{width:100%;padding:12px;margin:8px 0;border:1px solid #ddd;border-radius:6px;box-sizing:border-box;}
#contactForm button{background:<?php echo $buttonColor; ?>;color:white;border:none;padding:12px;border-radius:6px;cursor:pointer;font-weight:bold;}
.captcha{display:flex;align-items:center;gap:10px;margin:10px 0;}
#contactResult{margin-top:10px;padding:10px;border-radius:6px;font-weight:bold;}
</style>

<script>
document.getElementById('contactButton').onclick = function() {
    const form = document.getElementById('contactForm');
    form.style.display = form.style.display === 'none' ? 'block' : 'none';
};

document.getElementById('contactFormReal').onsubmit = function(e) {
    e.preventDefault();
    const formData = new FormData(this);
    formData.append('action', 'send_contact');  // Изменили на action

    fetch('', {
        method: 'POST',
        body: formData
    })
    .then(r => r.json())
    .then(data => {
        document.getElementById('contactResult').innerHTML = data.message;
        document.getElementById('contactResult').style.background = data.success ? '#d4edda' : '#f8d7da';
        if (data.success) this.reset();
    });
};
</script>